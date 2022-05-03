use std::fmt::Write;
use lazy_regex::regex;
use regex::Match;

#[no_mangle]
pub unsafe extern fn reformat(input_buf: *const u8, input_len: usize, output_buf: *mut u8, output_capacity: usize) -> isize {
    let input = std::str::from_utf8_unchecked(std::slice::from_raw_parts(input_buf, input_len));
    let ouput = std::slice::from_raw_parts_mut(output_buf, output_capacity);
    let mut write_to_output = WriteToFixedUtf8Buf::from(ouput);
    match reformat_rs::<_, &mut WriteToFixedUtf8Buf>(input, &mut write_to_output) {
        Ok(_) => {
            write_to_output.pos as isize
        },
        Err(_) => {
            -1
        }
    }
}

pub fn reformat_rs<I, W>(input: &str, output: I) -> std::fmt::Result where W: Write, I: Into<W> {
    let mut output = output.into();
    let pattern: &lazy_regex::Lazy<lazy_regex::Regex> = regex!(r"(?P<before>[.!?:-])?([\t\p{Z}--\p{Zl}]+)?(?P<break>(\r?[\n\f\u0085\u2028\u2029][\t\p{Z}--\p{Zl}]*)+)(?P<after>\p{Lu}\w+([\t\p{Z}--\p{Zl}]*\w+){0,9}:)?");

    // Don't want to use .replace_all because we already have an output buffer
    let mut copied_up_to = 0usize;
    for m in pattern.captures_iter(input) {
        let start = m.get(0).unwrap().start();
        let end = m.get(0).unwrap().end();

        // Copy unmatched part between the last match and this match
        if start > copied_up_to {
            output.write_str(&input[copied_up_to..start])?;
        }

        match (m.name("before"), m.name("break"), m.name("after")) {
            // Case 1: Hyphen at the end of the line => remove the hyphen and line break
            (Some(before), _,after) if before.as_str().contains("-") => {
                write_skipping_char(&mut output, '-', before.as_str())?;
                copy_optional_match(&mut output, &after)?;
            }
            // Case 2: One of the before/after triggers tells us to keep/normalize this break
            (Some(before), _,after) if !before.range().is_empty() => {
                output.write_str(before.as_str())?;
                output.write_str("\n\n")?;
                copy_optional_match(&mut output, &after)?;
            }
            (before,_,Some(after)) if !after.range().is_empty() => {
                copy_optional_match(&mut output, &before)?;
                output.write_str("\n\n")?;
                output.write_str(after.as_str())?;
            }
            (_,Some(break_m), _) if contains_more_than_one_line_break(break_m.as_str()) => {
                output.write_str("\n\n")?;
            }
            // Case 3 (default): Remove the break
            (before,_,after) => {
                copy_optional_match(&mut output, &before)?;
                output.write_str(" ")?;
                copy_optional_match(&mut output, &after)?;
            }
        }

        // Mark that we have processed the input bytes for this match
        copied_up_to = end;
    }

    // Copy unmatched part after the last match
    if copied_up_to < input.len() {
        output.write_str(&input[copied_up_to..])?;
    }

    Ok(())
}

fn write_skipping_char<W>(output: &mut W, skip: char, input: &str) -> std::fmt::Result where W: Write {
    let mut copied_up_to = 0usize;
    while copied_up_to < input.len() {
        let next_stop_at = input[copied_up_to..].find(skip).unwrap_or(input.len());
        output.write_str(&input[copied_up_to..next_stop_at])?;
        copied_up_to = next_stop_at + 1;
    }
    Ok(())
}

fn copy_optional_match<W>(mut output: W, opt_match: &Option<Match>) -> std::fmt::Result where W: Write {
    opt_match.map(|m| output.write_str(m.as_str())).unwrap_or(Ok(()))
}

fn contains_more_than_one_line_break(input: &str) -> bool {
    let mut have_seen_line_break = false;
    for c in input.as_bytes() {
        if *c == b'\n' {
            if have_seen_line_break {
                return true;
            }
            have_seen_line_break = true;
        }
    }
    false
}

struct WriteToFixedUtf8Buf<'a> {
    buf: &'a mut [u8],
    pos: usize,
}

impl<'a> From<&'a mut [u8]> for WriteToFixedUtf8Buf<'a> {
    fn from(buf: &'a mut [u8]) -> Self {
        WriteToFixedUtf8Buf {
            buf,
            pos: 0,
        }
    }
}

impl<'a, 'b> Write for &'b mut WriteToFixedUtf8Buf<'a> {
    fn write_str(&mut self, s: &str) -> std::fmt::Result {
        // Caller is responsible to pre-allocate a buffer of sufficient size.
        if self.pos + s.len() > self.buf.len() {
            return Err(std::fmt::Error);
        }
        self.buf[self.pos..self.pos + s.len()].copy_from_slice(s.as_bytes());
        self.pos += s.len();
        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn single_line_nothing_should_change() {
        // ///// GIVEN /////
        let input = "Nothing should change in a single line.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "Nothing should change in a single line.");
    }

    #[test]
    fn empty_input_works() {
        // ///// GIVEN /////
        let input = "";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "");
    }

    #[test]
    fn insufficiently_large_output_buffer_returns_err() {
        // ///// GIVEN /////
        let input = "This is a large input string.";
        let mut buf = [0u8; 1];
        let mut output = WriteToFixedUtf8Buf::from(&mut buf[..]);

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Err(std::fmt::Error));
        assert_eq!(buf[0], 0); // should not have touched the buffer
    }

    #[test]
    fn line_break_in_the_middle_of_a_sentence_should_be_removed() {
        // ///// GIVEN /////
        let input = "Line break in\nthe middle of a sentence.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "Line break in the middle of a sentence.");
    }

    #[test]
    fn hyphenation_should_be_removed() {
        // ///// GIVEN /////
        let input = "Dashes that are in- \n volved in splitting.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "Dashes that are involved in splitting.");
    }

    #[test]
    fn exotic_line_break_in_the_middle_of_a_sentence_should_be_removed() {
        // ///// GIVEN /////
        let input = "Line break in\r\nthe middle of a sentence.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "Line break in the middle of a sentence.");
    }

    #[test]
    fn double_line_break_in_the_middle_of_a_sentence_should_be_retained() {
        // ///// GIVEN /////
        let input = "Line break in\n\nthe middle of a sentence.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "Line break in\n\nthe middle of a sentence.");
    }

    #[test]
    fn break_after_exclamation_should_be_retained() {
        // ///// GIVEN /////
        let input = "End! \n And beginning.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "End!\n\nAnd beginning.");
    }

    #[test]
    fn break_after_full_stop_should_be_retained() {
        // ///// GIVEN /////
        let input = "End. \n And beginning.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "End.\n\nAnd beginning.");
    }

    #[test]
    fn break_after_colon_should_be_retained() {
        // ///// GIVEN /////
        let input = "End: \n And beginning.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "End:\n\nAnd beginning.");
    }


    #[test]
    fn break_after_question_should_be_retained()
    {
        // ///// GIVEN /////
        let input = "End? \n And beginning.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "End?\n\nAnd beginning.");
    }

    #[test]
    fn break_before_uppercase_then_colon_should_be_retained() {
        // ///// GIVEN /////
        let input = "1577 AZ \n Weight: 1.5 Kg.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "1577 AZ\n\nWeight: 1.5 Kg.");
    }

    #[test]
    fn break_before_lowercase_then_colon_should_be_removed() {
        // ///// GIVEN /////
        let input = "only one thing \n to do: hold the door.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "only one thing to do: hold the door.");
    }

    #[test]
    fn double_exotic_line_break_in_the_middle_of_a_sentence_should_be_normalized() {
        // ///// GIVEN /////
        let input = "Line break in\r\n\r\nthe middle of a sentence.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "Line break in\n\nthe middle of a sentence.");
    }

    #[test]
    fn triple_line_break_in_the_middle_of_a_sentence_should_be_normalized() {
        // ///// GIVEN /////
        let input = "Line break in\n\n\nthe middle of a sentence.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "Line break in\n\nthe middle of a sentence.");
    }

    #[test]
    fn double_line_break_with_spaces_should_be_normalized() {
        // ///// GIVEN /////
        let input = "Line break in   \n \t\n  the middle of a sentence.";
        let mut output = String::new();

        // ///// WHEN //////
        let result = reformat_rs::<_, &mut _>(input, &mut output);

        // ///// THEN //////
        assert_eq!(result, Ok(()));
        assert_eq!(output, "Line break in\n\nthe middle of a sentence.");
    }

    #[test]
    fn c_wrapper_smoke_test_success() {
        // ///// GIVEN /////
        let input = "Line break in\nthe middle of a sentence.";
        let mut buf = [0u8; 1024];

        // ///// WHEN //////
        let result = unsafe { reformat(input.as_ptr(), input.len(), buf.as_mut_ptr(), buf.len()) };

        // ///// THEN //////
        assert_eq!(result, 39);
        assert_eq!(String::from_utf8_lossy(&buf[..result as usize]), "Line break in the middle of a sentence.");
        assert_eq!(buf[39], 0);
    }

    #[test]
    fn c_wrapper_smoke_test_buffer_too_small() {
        // ///// GIVEN /////
        let input = "Line break in\nthe middle of a sentence.";
        let mut buf = [0u8; 12];


        // ///// WHEN //////
        let result = unsafe { reformat(input.as_ptr(), input.len(), buf.as_mut_ptr(), buf.len()) };

        // ///// THEN //////
        assert_eq!(result, -1);
        assert_eq!(buf[0], 0u8);
    }

}
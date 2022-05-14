---
# try also 'default' to start simple
theme: default
# random image from a curated Unsplash collection by Anthony
# background: https://source.unsplash.com/collection/94734566/1920x1080
# apply any windi css classes to the current slide
class: 'text-center'
# https://sli.dev/custom/highlighters.html
highlighter: prism
# show line numbers in code blocks
lineNumbers: false 
# some information about the slides, markdown enabled
info: |
  Using the Rust regex crate from C#
# persist drawings in exports and build
drawings:
  persist: false
---
 
# Using <rust-logo /> `regex` from <logos-c-sharp />

When .NET regular expressions aren't fast enough

<div class="abs-br m-6 flex gap-2">
  <a href="https://github.com/chklauser/ReformatOnCopy" target="_blank" alt="GitHub"
    class="text-xl icon-btn opacity-50 !border-none !hover:text-white">
    <carbon-logo-github />
  </a>
</div>

<!--
-->
---

# The Challenge

## **ReformatOnCopy**, a utility that 
 - monitors the clipboard for text <twemoji-magnifying-glass-tilted-right />
 - <span style="background-color: #575402; padding: 4pt">**removes *unnecessary* line breaks** <twemoji-brick /></span>
 - updates the clipboard <twemoji-clipboard />

## Interactive = "imperceptibly quick"
- **< 50ms** ideally <twemoji-racing-car />
- < 150ms at most <twemoji-hourglass-done />

---

# How hard could it be? <twemoji-flexed-biceps />
I know regular expressions! <twemoji-nerd-face />

<img src="https://imgs.xkcd.com/comics/regular_expressions.png" alt="Everybody stand back. I know regular expressions." 
  style="width: 480px; height: 320px; object-fit: cover; object-position: 50% 100%">

([xkcd #208](https://xkcd.com/208/))

<!-- 
 - Perfect use case for regular expressions.
 - Writing by hand possible, but can't iterate as fast to adjust heuristics
-->

---
layout: two-cols
---
# Not hard, but SLOW <twemoji-snail />
How slow? Let's measure! <twemoji-bar-chart />

Using [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) (<twemoji-star-struck />), we get:

| Method      | Text  | Mean \[ms] | Error \[ms] | StdDev \[ms] |   P95 \[ms] | Ratio | Allocated |
|-------------|-------|-----------:|------------:|-------------:|------------:|------:|----------:|
| Interpreted | 6.5KB |   12,933.5 |       67.23 |        52.49 |    13,003.1 |  1.00 |    180 KB |
| [Compiled]  | 6.5KB |    3,750.4 |       17.16 |        14.33 | **3,773.3** |  0.29 |    177 KB |
|             |       |            |             |              |             |       |           |
| Interpreted | 1.2KB |      604.4 |        3.70 |         3.28 |       609.2 |  1.00 |     59 KB |
| [Compiled]  | 1.2KB |      170.3 |        1.79 |         1.49 |   **173.2** |  0.28 |     55 KB |

::right::
<img src="https://media1.giphy.com/media/l2JHVUriDGEtWOx0c/giphy.gif?cid=790b761145cc720525d3f4325ac4431a4394a8dc7e31a088&rid=giphy.gif&ct=g"
  style="height: 120px;"
/>

<style>
strong {
  background-color: #575402; padding: 4pt
}
</style>

<!-- 

 - 3.5s still a win compared to manual work
 - extremely noticeable when you are used to copy&paste working _instantly_

-->

[Compiled]: https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regexoptions?view=net-6.0

---
layout: two-cols
clicks: 6
---
# Regular Expression<br/>from Hell? <twemoji-smiling-face-with-horns />

```regex {all|4|3|2|1|5|all}
([.!?:])?
(-)?
([\t\p{Z}-[\p{Zl}]]+)?
((\r*?[\r\n\f\u0085\u2028\u2029][\t\p{Z}-[\p{Zl}]]*)+)
(\p{Lu}\w+([\t\p{Z}-[\p{Zl}]]*\w+){0,9}:)?
```

<ul>
  <li v-click="1">match one or more unicode line breaks<br />(at least one mandatory)</li>
  <li v-click="2">eat preceding whitespace (`\s` would include `\n`)</li>
  <li v-click="3">detect hyphenation (`-`)</li>
  <li v-click="4">detect end of sentence before break</li>
  <li v-click="5">detect `:` within first couple of words of next sentence</li>
</ul>

::right::

<div v-click="6">

Eh, medium spicey <twemoji-hot-pepper /><twemoji-hot-pepper />

<ul style="list-style-type: none">
 <li><twemoji-green-circle /> single mandatory, non-empty group</li>
 <li><twemoji-red-circle /> arbitrary number of whitespace characters</li>
 <li><twemoji-red-circle /> arbitrary number of characters in words</li>
</ul>

</div>

<!-- 

0: a really bad regular expression? Let's see
1: match break
2: eat preceding whitespace
3: detect hyphenation
4: detect end of sentence before break
5: detect `:` within first couple of words of next sentence
6: analysis; could write this extremely efficiently by hand 
  - scan for break and then extend search left and right
  - actually a bit of shock that .NET regex does _this_ badly

What's going on?

-->

---

# Enhance! <twemoji-microscope />
Run under a profiler ([`dotnet trace` via event pipe](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace)) 
and analyze in [SpeedScope](https://www.speedscope.app/)

<img src="/profiler-compiled.png" alt="flame graph showing that almost 100% of the time is spent in the regular expression." />

<twemoji-backhand-index-pointing-right /> almost 100% of time spent in the regular expression <twemoji-loudly-crying-face />

<!-- 

 Can we do better?

-->

--- 

# Can we do better?
I heard [ripgrep](https://github.com/BurntSushi/ripgrep) is pretty quick

Potential alternative regular expression engines:

 - [RE2](https://github.com/google/re2) <logos-c-plusplus />, also used in <logos-go />
 - [Boost.Regex](http://www.boost.org/libs/regex/doc/html/) <logos-c-plusplus />
 - [PCRE](http://www.pcre.org/) <logos-c />, derived from Perl <logos-perl />
 - [ICU](http://www.icu-project.org/userguide/regexp.html) <logos-c-plusplus />, used in Swift <logos-swift />
 - **[`regex`](https://github.com/rust-lang/regex) <rust-logo />**

[A random benchmark on the internet](https://github.com/mariomka/regex-benchmark) says: the Rust `regex` library is pretty quick. 
Let's give it a shot!


<style>
strong {
  background-color: #575402; padding: 4pt
}
</style>

<!-- 
There are a number of regular expression libraries out there. Most are written in C/C++.

 - RE2 and Boost.Regex are often used over the C++ standard library regex engine
 - PCRE is basically as old as time
 - ICU has a strong focus on correctness, especially with regards to Unicode
 - and then there is the de-facto standard `regex` library for rust

-->

---

# A tiny Rust library <twemoji-rocket />

```bash
cargo new --vcs none --lib libreformat
cd libreformat
cargo add regex
```

edit `Cargo.toml` to produce a C-compatible, dynamically linked library (DLL/SO/dylib) <twemoji-books />
```toml {2-3}
...
[lib]
crate-type = ["cdylib"]
...
```

implement our transformation function in Rust <twemoji-racing-car />
```rust {1,3}
pub fn reformat_rs<W>(input: &str, output: I) -> std::fmt::Result where W: std::fmt::Write {
  // left as exercise for the reader
}
```

But how do we call this from C#? <twemoji-thinking-face />

---

# Use the ~~force~~ FFI <twemoji-speech-balloon /> (1/4)
Expose a C-compatible<sup>1</sup> function

```rust {all|1-2|2-5|2,6-7|9-10|10-17|all}
#[no_mangle]
pub unsafe extern fn reformat(
      input_buf: *const u8, input_len: usize, 
      output_buf: *mut u8, output_capacity: usize) 
-> isize {
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
```

<small>1: No NUL-terminated C-strings, but byte buffers known to contain UTF-8</small>

<!-- 
 0. Expose a C-compatible function
 1. no_mangle + extern
 2. signature
 3. wrap input in Rust types (unsafe!)
 4. call Rust function
 5. return result
 6. and that's it!
-->

---

# Use the FFI <twemoji-speech-balloon /> (2/4)
```bash
# Build the Rust library in release mode
cargo build --release
```
and have MSBuild place the DLL/SO/dylib in the dotnet output directory
```xml
<ItemGroup>
    <NativeLibs Include="$(MSBuildThisFileDirectory)\libreformat\target\release\*.dll" />
    <NativeLibs Include="$(MSBuildThisFileDirectory)\libreformat\target\release\*.so" />
    <NativeLibs Include="$(MSBuildThisFileDirectory)\libreformat\target\release\*.dylib" />
    <None Include="@(NativeLibs)">
        <Link>%(RecursiveDir)%(FileName)%(Extension)</Link>
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
</ItemGroup>
```
On to the actual C# part!

--- 

# Use the FFI <twemoji-speech-balloon /> (3/4)
En-/decode to and from UTF-8 (C# uses UTF-16 for its strings <twemoji-loudly-crying-face />) 

```csharp {all|1-2|7-8,21|4-5,9-14,18-19|all}
    [DllImport("libreformat", EntryPoint = "reformat", ExactSpelling = true)]
    static extern unsafe nint reformat(byte* inputBuf, int inputLen, byte* outputBuf, int outputCapacity);

    private readonly ArrayPool<byte> pool = ArrayPool<byte>.Create(32 * 1024 * 1024, 50);
    private readonly Encoder encoder = Encoding.UTF8.GetEncoder();

    private string removeLineBreaksViaRust(string input)
    {
        // Encode input as UTF-8 into the input buffer 
        var inputBuf = pool.Rent(Encoding.UTF8.GetMaxByteCount(input.Length));
        var effectiveUtf8ByteCount = encoder.GetBytes(input.AsSpan(), inputBuf.AsSpan(), true);
        // Output can be at most 1.5 times the input size (and only in terms of ASCII characters)
        var outputBuf = pool.Rent(effectiveUtf8ByteCount + effectiveUtf8ByteCount / 2 +
            (effectiveUtf8ByteCount % 2 == 0 ? 0 : 1));

        // P/Invoke magic -> see next slide

        pool.Return(inputBuf);
        pool.Return(outputBuf);
        return output;
    }
```

--- 

# Use the FFI <twemoji-speech-balloon /> (4/4)

```csharp {all|8-12,18-19|13|14-15|17,21|all}
    [DllImport("libreformat", EntryPoint = "reformat", ExactSpelling = true)]
    static extern unsafe nint reformat(byte* inputBuf, int inputLen, byte* outputBuf, int outputCapacity);

    private string removeLineBreaksViaRust(string input)
    {
        // ... prepare `inputBuf` and `outputBuf` ...
        string output;
        unsafe
        {
            fixed (byte* inputPtr = inputBuf)
            fixed (byte* outputPtr = outputBuf)
            {
                var result = reformat(inputPtr, effectiveUtf8ByteCount, outputPtr, outputBuf.Length);
                if (result < 0)
                    throw new("Error during line break detection. Error code: " + result);

                output = new((sbyte*)outputPtr, 0, (int)result, Encoding.UTF8);
            }
        }
        // ... return `inputBuf` and `outputBuf` to the pool ...
        return output;
    }
```

---
clicks: 1
---
# But is it actually fast? <span v-click="1" class="reserve">OH YES! <twemoji-smiling-face-with-sunglasses /></span>

| Method | Text   | Mean <span v-click="1" class="cool">\[μs]</span><span v-click-hide="1">\[μs]</span> | Error <span v-click="1" class="cool">\[μs]</span><span v-click-hide="1">\[μs]</span> | StdDev <span v-click="1" class="cool">\[μs]</span><span v-click-hide="1">\[μs]</span> |   P95 <span v-click="1" class="cool">\[μs]</span><span v-click-hide="1">\[μs]</span> | Ratio |  Gen 0 | Allocated |
|--------|--------|-----------------------------------------------------------------------------------:|------------:|-------------:|------------:|------:|-------:|----------:|
| .NET   | 6.5 KB |  3,860,886.3 |   61,330.57 |    57,368.65 | 3,964,122.8 | 1.000 |      - |    177 KB |
| Rust   | 6.5 KB |        883.6 |       12.06 |        11.28 |       899.5 | 0.000 | 0.9766 |     13 KB |
|        |        |              |             |              |             |       |        |           |
| .NET   | 5.1 KB |  2,182,444.1 |   27,576.73 |    24,446.05 | 2,230,336.6 | 1.000 |      - |    172 KB |
| Rust   | 5.1 KB |        645.3 |        2.31 |         1.93 |       648.9 | 0.000 | 3.9063 |     34 KB |
|        |        |              |             |              |             |       |        |           |
| .NET   | 1.2 KB |    171,958.6 |      872.86 |       681.47 |   172,782.8 | 1.000 |      - |     55 KB |
| Rust   | 1.2 KB |        130.5 |        1.71 |         1.60 |       133.3 | 0.001 | 1.7090 |     16 KB |

<style>
strong, .cool {
  background-color: #575402; 
}
.slidev-vclick-hidden:not(.reserve) {
  display: none;
}
</style>

---

# What have I learned? 

 - [BenchmarkDotNet](https://benchmarkdotnet.org/) is **excellent** <twemoji-star-struck />
 - Profile (or run experiments) **_before_** you optimize <twemoji-microscope />
   - Other regular expressions perform perfectly adequately in .NET <twemoji-index-pointing-up /><twemoji-nerd-face />
 - `dotnet trace` is pretty cool (but not super straightforward to use) <twemoji-lab-coat />
 - [SpeedScope](https://www.speedscope.app/) can visualize traces <twemoji-bar-chart />
 - Create **test suite *before*** you optimize  <twemoji-police-car-light />
 - Integration between Rust and C# was **surprisingly painless** <rust-logo /><twemoji-handshake /><logos-c-sharp />


---
layout: center
class: text-center
---

# Thanks <twemoji-waving-hand />

[Source code on GitHub <carbon-logo-github />](https://github.com/chklauser/reformatoncopy)

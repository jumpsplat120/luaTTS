# luaTTS
 Implementation of microsoft's and cereproc's text to speech engine using Love2D

## Installation
Drop the bin folder into your project. Feel free to name it whatever you'd like. require main. Keep `TTS.dll` and `cerevoice_eng.dll` next to each other and main. Keep `WinReg.dll` next to the luareg `main.lua`. Make sure there is an `audio` folder at the top level of this project, next to `main.lua` and `TTS.dll` and so on. 

*Fused Installation*
If you have fused the project, move all dll's (`WinReg.dll`, `TTS.dll`, and `cerevoice_eng.dll`) to the top most level of the project, next to the .exe. Make sure that there is an `audio` folder as well at the top level.

Place `tts:quit()` into `love.quit()` to clean up all generated .wav's once your application is closed, or not, if you wanted to keep the wav's around.

Make sure to `tts:load()` before doing anything.

## Dependencies
Written for Love2D. Might be able to get away with having it work with just luaJIT, if you can replace the love functions, and if you have a way to run tts:quit() right before closing the application, since it handles removing all generated audio files. Can't help you if you do that though, because it'll be finicky as all get out.

Needs WinReg, a project that allows easy windows registry manipulation using Lua. Used to get the file path to cereproc voices.
Needs getClassic, my fork of classic.lua that has getters and setters. Comes prepackaged.

Windows only, since it accesses the window's TTS engine, and uses the windows registry to load voice information for the cereproc engine.

## Notes
Might just be me, but the code feels very messy unfortunately. It is using a custom C# dll I wrote that gives me access to the microsoft TTS and the cereproc engine, but unfortunately, the engine's are slighly different, so there's this case of some tags only work with one engine and not the other, even though they are both supposedly implemeneting v1 of the SSML spec. As well, there were a number of tags that aren't implemented due to being a bit unclear in the spec, or because they weren't particuarly helpful in the context I have set up. For example, the mark tags aren't useful, since they would send information to the engine when that word is said, but the implementation converts the text into an audio file which is loaded in love, so there's no way of getting that info. Or the alias tag, which is only really useful if we were displaying the XML as well as speaking it.

The cereproc engine also has it's own tags, which are listed down below as cereproc specific. Trying to run tags that are only for cere voices will fail gracefully; the code will simply return the plain text, and not try to create a tag for the MSTTS engine. So **theoretically** you should be able to use the same text strings for both engines.

Only uses 64 bit voices SAPI5 voices! A 32bit voice will not load, nor will Azure Speech voices.
## Usage

### tts:load()
Loads everything, and sets a default voice from preinstalled voices.

### tts:quit()
Removes all generated `.wav` files at the end of the session. If you want/need to keep the files around for any reason, just don't use this. Otherwise, call it right before closing the application.

### tts:speak(ssml, callback)
Main function. Automatically wraps everything in 'speak' tags cause that's the highest level required tag. Returns a love audioSource that you can do whatever you want with. Files will be saved in a folder, and will be cleaned on shutdown.

### tts.sample_rate
#### get
Returns the sample rate, used by the microsoft tts engine, divided by 1000. so you would get 44.1 instead of 441000, for example.
#### set
Set the sample rate used exclusively by the microsoft tts engine. By default, this is set to 48khz. If you need to change this for any reason, simply pass in the sample rate, divided by 1000. So for example, for a sample rate of 44.1khz, you'd pass in 44.1. Expected values: 8, 11, 12, 16, 22, 24, 32, 44.1, 48

### tts.bit_depth
#### get
Returns the bit depth, used by the microsoft tts engine.
#### set
Set the bit depth used exclusively by the microsoft tts engine. By default, this is set to 16. Expects a value of either 8 or 16.

### tts.channel_mode
#### get
Returns the channel mode, used by the microsoft tts engine.
#### set
Set the channel mode used exclusively by the microsoft tts engine. By default, this is set to stereo. Expects a value of either `mono` or `stereo`. Not case sensitive.

### tts.voice
#### get
Returns the the name of the currently used voice. This will be the full name of a voice, rather than just the personality name. So for example, the Microsoft David voice is referred to as `Microsoft David Desktop`.
#### set
Set the currently used voice. Uses pattern matching, so you don't need to input the entire name; for example, if you wanted to use the Microsoft David voice, simply `David` would do. If the voice you use is detected as a CereProc voice, it will gather the needed information to load that voice, by checking the windows registry for the install location. If, for whatever reason, the CereProc voice location in the registry does not match the current install location, or you'd like to load a Cereproc voice manually, you can do the following:

```lua

--You can probably skip this, since the voice name is only ever used internally to load 
--the correct engine and set the voice with microsoft's tts

self.data.voice = "FULL_NAME_OF_VOICE"
self.data.cereproc = {}

--This will be the major version of this voice. Most likely either 5 or 6. If you 
--look at the folder, it's probably formatted like "CereVoice NAME ver_num.x.x". 
--The major version number is what we need here
self.data.cereproc.ver = ver_num 

self.data.engine_type = "cereproc"

self.data.cereproc.folder = "path/to/folder" -- We only want the containing folder, not any specific file here.
				
file = io.popen('dir "' .. self.data.cereproc.folder .. '" /b')

--You can also load these in one at a time, if you need to. If you have you your version < 6, then you'll 
--only have a .lic and a .voice. If it's 6 or above, you'll need all of these files.
for item in file:lines() do
 if item:find(".lic$") then
  self.data.cereproc.lic = item
 elseif item:find(".voice$") then
  self.data.cereproc.voice = item
 elseif item:find(".pem$") then
  self.data.cereproc.root = item
 elseif item:find(".crt$") then
  self.data.cereproc.cert = item
 elseif item:find(".key$") then
  self.data.cereproc.key = item
 end
end

file:close()
```

### tts.voices
#### get
Returns a table of installed voices, by name. You can pass any one of the voices into `tts.voice` to load it directly.
#### set
Only throws an error. You can't set the voices table since it doesn't actually exist. It is generated on call from the `TTS.dll`, which returns a list of installed voices whenever accessed.

### tts:pause(length)
Returns a break tag for a specifc length of time in ms. If no length is passed, tag will proceed without it, and pause for as long as the engine deems appropriate. Expected values: 1 -> math.huge

### tts:emphasis(text, strength)
Returns an emphasis tag for a word, with an assosciated strength. If no strength is passed, defaults to a strong emphasis, which is one up from default. If strength is "none", it basically tells the engine not to put any emphasis there, even if it wanted to. Expected values: "strong", "moderate", "reduced", "none"

### tts:audio(file, fallback)
Returns an audio tag, which will play a .wav audio file, and contains a text fallback in case the clip is unable able to be recieved. VERY finicky tag, can cause the engine to just straight up throw an exception. -My suggestion is to use love.audio for extra audio you want playing during the clip, instead of using this tag. Also, this tag only seems to work with the microsoft engine, contrary to what Cereproc's docs said. You've been warned. EDIT: Got an email that said it only works as a closed tag. Still doesn't work lol.

### tts:pitch(t, value)
Helper function that forms a pitch string. Used in conjunction with the `prosody` tag. `t` is type, and expects a string of "hertz", "percent", or "level". Value is the value of the pitch, either a string or number for level, or number if type is hertz or percentage. If using a number, value will be rounded to closest interger value. If type is level, either a string representing the level ("default", "x-low", "low", "medium", "high", "x-high") or a number between 1 - 6 to access one of the values in the array.

### tts:range(t, value)
Helper function that forms a range string. Used in conjunction with the `prosody` tag. Identical rules to `tts.pitch`.

### tts:contour(...)
Helper function to form a contour string. Used in conjuction with the `prosody` tag. Takes up to 5 tables, each one formatted in the following; { t, pos, value }, where t is a string 'hertz' or 'percent', value is a number value, and pos is a number value between 0 and  100. Both the value and position will be rounded to the nearest whole number, and the position will be clamped between 0 and 100.

### tts:rate(value, array)
Helper function that forms a rate string. Used in conjunction with the `prosody` tag. Value is either a string containing one of the accepted string rates ("default", "x-low", "low", "medium", "high", "x-high"), or it can be a non negative number value, which will be a percentage, and act as a multiplier to the speed at which the voice is spoken. So for example, 50% is half as fast as normal, while 200% would be twice as fast as normal. If true is passed to the second value, the value will instead be used as a value to retrieve one of the above string rates from the array. ie 1 == default, 2 == x-low, and so on. The number values will be automatically rounded to the nearest whole number value.

### tts:duration(value)
Helper function that forms a duration string. Used in conjunction with the `prosody` tag. Value is rounded, and must be greater than 0. Returns a tag that specifies the length of time the engine should take to speak a portion of text, in milliseconds. Takes higher precendence than rate.

### TTS:volume(value, array)
Helper function that forms a volume string. Used in conjunction with the `prosody` tag. Value can either be a number or a string. If it is a string, expected values are "default", "silent", "x-soft", "soft", "medium", "high" and "x-high". If it is a number, then it is a value in decibels, telling the engine how loud a portion of text should be. If the array parameter is true, then value is instead used as an index to retrieve one of the string values instead.

### tts:prosody(text, pitch, contour, range, rate, duration, volume)
Forms a prosody tag. Basically an all in one tag for each of the values listed as parameters. All values are technically optional, although at least one value does need to be passed. If all are nil, then the text is simply returned with a tag. If you'd like to use one value, set the preceding values to nil. For example, if you want to use contour, set pitch to `nil`.

### tts:sub(value, replace)
Not particularly useful oob, but tells the engine to say `replace` instead of `value`. Only really useful if you are visually rendering the xml in some way.

### tts:spurt(id, index)
Returns a spurt tag for a specific sound. If a spurt is an array, then it will pick one at random. If you pass an index, it will pick that one specifically. Will fail gracefully; ie, if the index isn't an option, then it returns the first valid value, and if the id isn't valid, will simply return an empty string.
**CEREPROC TAG**

### tts:variant(value, text)
Returns a variation of the spoken word or phrase, based off the numeric value passed. Value will be rounded to the nearest whole number, and clamped between 0 and math.huge.
**CEREPROC TAG**

### tts:emotion(emote, text)
Returns an emotion tag. Note that only certain cereproc voices support the emotion tag; currently listed at Adam, Caitlin, Heather, Isabella, Jack, Jess, Katherine, Kirsty, Laura, Sarah, Stuart, Suzanne and William. If emote passed is not a valid emote, simply returns the text. Does *not* check for valid voices.
**CEREPROC TAG**

## Not implemented

### tts.phoneme()
Allows you to use the IPA or some other phoneme set. Cereproc also has their own phoneme set, and it will automatically convert the unicode into the cereproc set if that's the voice you're using. Array? Unicode str? Using utf8?

### tts.sayAs()
Not entirely clear what the say-as tag does and how it differs from say, phoneme. I'll get around to it when I understand what the w3 spec was saying.

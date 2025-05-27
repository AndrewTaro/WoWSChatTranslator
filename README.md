# WoWS Chat Translator
This application imports the chat messages from World of Warships, and sends back the translated texts.  
It is intended to work with [TTaro Chat](https://github.com/AndrewTaro/TTaroChat).

- The settings, such as selected language, API key, etc, will be automatically saved in your local computer.
  - The API key is stored in Windows Credential Manager.
  - The rest of the preferences are saved in `userSettings.json` in the same directory as the `.exe`.

# Requirements
You must install the mod in World of Warships.
- [TTaro Chat](https://github.com/AndrewTaro/TTaroChat)

You also need an API key from DeepL.
- The free plan is sufficient for chat translations (500,000 characters per month).
- For details, please check [DeepL](https://www.deepl.com/ja/home)

# How to Use
1. Download `WoWSChatTranslator.exe` from the latest release.
2. Install [TTaro Chat](https://github.com/AndrewTaro/TTaroChat) in World of Warships.
4. Double click `WoWSChatTranslator.exe` to launch.
5. Enter your DeepL API key.
6. Select the language in which you'd like to translate.
7. Now, the messages in battle will be translated!

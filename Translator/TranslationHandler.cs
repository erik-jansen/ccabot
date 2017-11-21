﻿using LuisBot.Utilities;
using Microsoft.Bot.Connector;

namespace LuisBot.Translator
{
    public static class TranslationHandler
    {
        public static string DetectAndTranslate(Activity activity)
        {
            //detect language
            //update state for current user to detected language
            var inputLanguageCode = DoLanguageDetection(activity.Text);

            StateHelper.SetUserLanguageCode(activity, inputLanguageCode);

            if (inputLanguageCode.ToLower() != "en")
            {

                return DoTranslation(activity.Text, inputLanguageCode, "en");

            }
            return activity.Text;
        }

        public static string DoTranslation(string inputText, string inputLocale, string outputLocale)
        {
            var translator = new Translator();
            var translation = translator.Translate(inputText, inputLocale, outputLocale);
            return translation;
        }

        private static string DoLanguageDetection(string input)
        {
            var translator = new Translator();
            return translator.Detect(input);
        }

    }
}
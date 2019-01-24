﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GithubService.Models;
using GithubService.Services.Interfaces;

namespace GithubService.Services.Parsers
{
    public class FileParser : IFileParser
    {
        public CodeFile ParseContent(string filePath, string content)
        {
            CheckNullOrEmptyArguments(filePath, content);

            var codeSampleFile = new CodeFile
            {
                FilePath = filePath
            };

            var extractedLanguage = GetLanguage(filePath);
            if (extractedLanguage == null)
            {
                return codeSampleFile;
            }
            var language = (CodeFragmentLanguage) extractedLanguage;

            var sampleIdentifiers = ExtractSampleIdentifiers(content, language);
            if (sampleIdentifiers.Count == 0)
            {
                return codeSampleFile;
            }

            ExtractCodeSamples(content, sampleIdentifiers, language, codeSampleFile);

            if (sampleIdentifiers.Count != codeSampleFile.CodeFragments.Count)
            {
                throw new ArgumentException($"Incorrectly marked code sample in file {filePath}");
            }

            return codeSampleFile;
        }

        private static void CheckNullOrEmptyArguments(string filePath, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException($"Content of file {filePath} is either empty or null");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Invalid file path");
            }
        }

        private static CodeFragmentLanguage? GetLanguage(string filepath)
        {
            var languageIdentifier = filepath.Split('/')[0];

            switch (languageIdentifier)
            {
                case "cUrl":
                    return CodeFragmentLanguage.CUrl;
                case "c#":
                    return CodeFragmentLanguage.CSharp;
                case "js":
                    return CodeFragmentLanguage.JavaScript;
                case "ts":
                    return CodeFragmentLanguage.TypeScript;
                case "java":
                    return CodeFragmentLanguage.Java;
                case "javarx":
                    return CodeFragmentLanguage.JavaRx;
                case "php":
                    return CodeFragmentLanguage.PHP;
                case "swift":
                    return CodeFragmentLanguage.Swift;
                case "ruby":
                    return CodeFragmentLanguage.Ruby;
                default:
                    return null;
            }
        }

        private static List<string> ExtractSampleIdentifiers(string content, CodeFragmentLanguage language)
        {
            var sampleIdentifiersExtractor = new Regex($"{language.GetCommentPrefix()} DocSection: (.*?)\n", RegexOptions.Compiled);
            var sampleIdentifiers = new List<string>();

            var matches = sampleIdentifiersExtractor.Matches(content);

            foreach (Match match in matches)
            {
                sampleIdentifiers.Add(match.Groups[1].Value.Trim());
            }

            return sampleIdentifiers;
        }

        private static void ExtractCodeSamples(
            string content,
            IReadOnlyList<string> sampleIdentifiers,
            CodeFragmentLanguage language,
            CodeFile codeFile)
        {
            var codeSamplesExtractor = GetCodeSamplesExtractor(sampleIdentifiers, language);
            var codeSamplesFileMatches = codeSamplesExtractor.Matches(content);

            var matchedGroupIndex = 1;
            for (var index = 0; index < codeSamplesFileMatches.Count; index++)
            {
                var sampleIdentifier = sampleIdentifiers[index];
                var matchedContent = codeSamplesFileMatches[index]
                    .Groups[matchedGroupIndex]
                    .Value
                    .Trim();

                if (matchedContent.Contains($"{language.GetCommentPrefix()} DocSection: "))
                {
                    throw new ArgumentException("Nested or intersected marking of code samples found");
                }

                matchedGroupIndex += 2;

                var (sampleType, sampleCodename) = ExtractCodenameAndTypeFromIdentifier(sampleIdentifier);
                codeFile.CodeFragments.Add(new CodeFragment
                    {
                        Codename = sampleCodename,
                        Type = sampleType,
                        Content = matchedContent,
                        Language = language
                    }
                );
            }
        }

        private static Regex GetCodeSamplesExtractor(IEnumerable<string> sampleIdentifiers, CodeFragmentLanguage language)
        {
            var codeSamplesExtractor = sampleIdentifiers.Aggregate("",
                (current, sampleIdentifier) => current + $"{language.GetCommentPrefix()} DocSection: {sampleIdentifier}\n*?((.|\n)*?){language.GetCommentPrefix()} EndDocSection|");

            codeSamplesExtractor = codeSamplesExtractor.Length > 0
                ? codeSamplesExtractor.Remove(codeSamplesExtractor.Length - 1)
                : codeSamplesExtractor;

            return new Regex(codeSamplesExtractor, RegexOptions.Compiled);
        }

        private static (CodeFragmentType sampleType, string sampleCodename) ExtractCodenameAndTypeFromIdentifier(string sampleIdentifier)
        {
            var index = sampleIdentifier.IndexOf('_');
            if (index <= 0)
            {
                throw new ArgumentException($"Unrecognized sample type in sample identifier {sampleIdentifier}.");
            }

            var type = sampleIdentifier.Substring(0, index);
            var codename = sampleIdentifier.Substring(index + 1, sampleIdentifier.Length - index - 1);

            return (GetSampleType(type), codename);
        }

        private static CodeFragmentType GetSampleType(string type)
        {
            switch (type)
            {
                case "single":
                    return CodeFragmentType.Single;
                case "multiple":
                    return CodeFragmentType.Multiple;
                default:
                    throw new ArgumentException($"Unsupported sample type {type}.");
            }
        }
    }
}

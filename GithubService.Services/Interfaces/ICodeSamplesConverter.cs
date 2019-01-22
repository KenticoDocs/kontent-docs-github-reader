﻿using System.Collections.Generic;
using GithubService.Models;
using GithubService.Models.KenticoCloud;

namespace GithubService.Services.Interfaces
{
    public interface ICodeSamplesConverter
    {
        IEnumerable<CodenameCodeFragments> ConvertToCodenameCodeFragments(IEnumerable<CodeFile> codeSampleFiles);

        CodeSamples ConvertToCodeSamples(CodenameCodeFragments codenameCodeFragment);
    }
}

using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace VideoManager.Validators
{
    public class VideoValidator : AbstractValidator<IFormFile>
    {
        private readonly HashSet<string> _acceptedFileFormats = new()
        {
            "video/mp4",
            "video/webm"
        };

        public VideoValidator()
        {
            RuleFor(x => x.Length).GreaterThan(0);
            RuleFor(x => x.Length).LessThanOrEqualTo(250000000);
            RuleFor(x => x.ContentType).Must(x => _acceptedFileFormats.Contains(x));
        }
    }
}

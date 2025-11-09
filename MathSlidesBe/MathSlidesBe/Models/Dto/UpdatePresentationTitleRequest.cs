using System;

namespace MathSlidesBe.Models.Dto;

public class UpdatePresentationTitleRequest
{
    public Guid PresentationId { get; set; }
    public required string NewTitle { get; set; }

}

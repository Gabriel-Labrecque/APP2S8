using Sanssoussi.Areas.Identity.Data;

namespace Sanssoussi.Models
{
    public class Comment
    {
        public const int MaxLength = 500;

        public string CommentId { get; set; }

        public string UserId { get; set; }

        public SanssoussiUser User { get; set; }

        public string Text { get; set; }
    }
}

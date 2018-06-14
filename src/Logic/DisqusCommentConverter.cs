using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Globals;

namespace DisqusImport.Logic
{
    /// <summary>
    /// Converts a single Disqus comment.
    /// </summary>
    public sealed class DisqusCommentConverter
    {
        private static readonly Regex AnchorFixer = new Regex(@"<a href=""([^""]+)"" rel=""[^""]+"" title=""[^""]+"">([^<]+)</a>");
        private readonly DisqusAuthorConverter _authorConverter;

        public DisqusCommentConverter(DisqusAuthorConverter authorConverter)
        {
            _authorConverter = authorConverter;
        }

        public string Filename(post post) => post.createdAt.Date.ToString("yyyy-MM-dd") + "-" + JsonModel.ConvertDisqusId(post.id1).ToString("D") + ".json";

        public async Task<JsonModel> ConvertAsync(string staticmanPostId, post post, string path)
        {
            var author = await _authorConverter.ConvertAsync(path, post);
            return new JsonModel
            {
                DisqusId = post.id1,
                DiqusParentId = post.parent?.id,
                PostId = staticmanPostId,
                Message = ConvertMessage(post.message),
                Date = post.createdAt,
                AuthorUserId = author.Username == null ? "" : "disqus:" + author.Username,
                AuthorName = author.Name ?? "",
                AuthorEmailMD5 = author.HashedEmail ?? "",
                AuthorEmailEncrypted = author.EncryptedEmail ?? "",
                AuthorUri = author.Url ?? "",
                AuthorFallbackAvatar = author.FallbackAvatar ?? "",
            };
        }

        private static string ConvertMessage(string message)
        {
            // Strip a@title attributes (which don't work with showdown), and undo Disqus link shortening.
            var html = AnchorFixer.Replace(message, match =>
            {
                if (match.Groups[2].Value.EndsWith("..."))
                {
                    var shortened = match.Groups[2].Value.Substring(0, match.Groups[2].Value.Length - 3);
                    if (match.Groups[1].Value.StartsWith(shortened))
                        return $"<a href=\"{match.Groups[1].Value}\">{match.Groups[1].Value}</a>";
                }

                return $"<a href=\"{match.Groups[1].Value}\">{match.Groups[2].Value}</a>";
            });
            return MarkdownConverter.Convert(html);
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace LuckyDev
{
    public sealed class CodeReviews
    {
        private Dictionary<string, int> _codeReviews;
        
        public CodeReviews()
        {
            _codeReviews = null;
        }

        private Dictionary<string, int> CodeReviewCollection
        {
            get
            {
                if (_codeReviews == null)
                {
                    _codeReviews = new Dictionary<string, int>();
                }

                return _codeReviews;
            }
        }

        public void AddProjectMember(string name)
        {
            if (!CodeReviewCollection.Keys.Contains(name))
            {
                CodeReviewCollection.Add(name, 0);
            }
        }

        public void ResetCodeReviews()
        {
            List<string> keyList = CodeReviewCollection.Keys.ToList();
            foreach (string name in keyList)
            {
                CodeReviewCollection[name] = 0;
            }
        }

        public void AddCodeReview(string name)
        {
            if (CodeReviewCollection.Keys.Contains(name))
            {
                CodeReviewCollection[name] += 1;
            }
        }

        public int GetCodeReviewsForTeamMember(string name)
        {
            if (CodeReviewCollection.Keys.Contains(name))
            {
                return CodeReviewCollection[name];
            }

            return -1;
        }

        public Dictionary<string, int> GetTopFive()
        {
            var returnDictionary = CodeReviewCollection
                                    .OrderByDescending(t => t.Value)
                                    .Take(5)
                                    .ToDictionary(p => p.Key, q => q.Value);

            return returnDictionary;
        }

        public List<string> GetTeamMembersWithoutCodeReviews()
        {
            List<string> listResult = (from entry in CodeReviewCollection where entry.Value == 0 select entry.Key).ToList();
            return listResult;
        }

        public List<string> GetTeamMembersWithLessCodeReviews()
        {
            int lessCodeReviewNumber = (from entry in CodeReviewCollection orderby entry.Value ascending select entry.Value).First(); 
            List<string> listResult = (from entry in CodeReviewCollection where entry.Value == lessCodeReviewNumber select entry.Key).ToList();
            return listResult;
        }
    }
}

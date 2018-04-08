using System.Collections.Generic;
using System.Linq;

namespace LuckyDev
{
    public sealed class CodeReviews
    {
        #region private members

        private Dictionary<string, int> codeReviews;

        #endregion 
        
        #region constructor

        public CodeReviews()
        {
            this.codeReviews = null;
        }

        #endregion

        #region private properties

        private Dictionary<string, int> CodeReviewCollection
        {
            get
            {
                if (this.codeReviews == null)
                {
                    this.codeReviews = new Dictionary<string, int>();
                }

                return this.codeReviews;
            }
        }

        #endregion

        #region public methods

        public void AddProjectMember(string name)
        {
            if (!this.CodeReviewCollection.Keys.Contains(name))
            {
                this.CodeReviewCollection.Add(name, 0);
            }
        }

        public void ResetCodeReviews()
        {
            List<string> keyList = this.CodeReviewCollection.Keys.ToList<string>();
            foreach (string name in keyList)
            {
                this.CodeReviewCollection[name] = 0;
            }
        }

        public void AddCodeReview(string name)
        {
            if (this.CodeReviewCollection.Keys.Contains(name))
            {
                this.CodeReviewCollection[name] += 1;
            }
        }

        public int GetCodeReviewsForTeamMember(string name)
        {
            if (this.CodeReviewCollection.Keys.Contains(name))
            {
                return this.CodeReviewCollection[name];
            }
            else
            {
                return -1;
            }
        }

        public Dictionary<string, int> GetTopFive()
        {
            var sortedDict = (from entry in this.CodeReviewCollection orderby entry.Value descending select entry).Take(5);
            
            // TODO: este paso seguramente se puede evitar, alguien que sepa linq seguramente sabe como hacerlo.
            Dictionary<string, int> returnDictionary = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> keyvalue in sortedDict)
            {
                returnDictionary.Add(keyvalue.Key, keyvalue.Value);
            }

            return returnDictionary;
        }

        public List<string> GetTeamMembersWithoutCodeReviews()
        {
            List<string> listResult = (from entry in this.CodeReviewCollection where entry.Value == 0 select entry.Key).ToList<string>();
            return listResult;
        }

        public List<string> GetTeamMembersWithLessCodeReviews()
        {
            int lessCodeReviewNumber = (from entry in this.CodeReviewCollection orderby entry.Value ascending select entry.Value).First(); 
            List<string> listResult = (from entry in this.CodeReviewCollection where entry.Value == lessCodeReviewNumber select entry.Key).ToList<string>();
            return listResult;
        }

        #endregion
    }
}

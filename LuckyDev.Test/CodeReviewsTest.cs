using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuckyDev.Test
{
    /// <summary>
    /// This is a test class for CodeReviewsTest and is intended
    /// to contain all CodeReviewsTest Unit Tests
    /// </summary>
    [TestClass()]
    public class CodeReviewsTest
    {
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return this.testContextInstance;
            }

            set
            {
                this.testContextInstance = value;
            }
        }

        #region Additional test attributes

        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}

        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}

        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}

        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        #endregion

        /// <summary>
        /// A test for ResetCodeReviews
        /// </summary>
        [TestMethod()]
        public void ResetCodeReviewsTest()
        {
            CodeReviews target = new CodeReviews();
            string teamMember01 = "Mario Moreno";
            string teamMember02 = "El cuco";
            target.AddProjectMember(teamMember01);
            target.AddProjectMember(teamMember02);
            target.AddCodeReview(teamMember01);
            target.AddCodeReview(teamMember02);
            target.AddCodeReview(teamMember02);
            target.AddCodeReview(teamMember02);
            target.ResetCodeReviews();
            Assert.AreEqual(0, target.GetCodeReviewsForTeamMember(teamMember01), "Al resetear los code review la cantidad deberia ser 0.");
            Assert.AreEqual(0, target.GetCodeReviewsForTeamMember(teamMember02), "Al resetear los code review la cantidad deberia ser 0.");
        }

        /// <summary>
        /// A test for GetTopFive
        /// </summary>
        [TestMethod()]
        public void GetTopFiveTest()
        {
            CodeReviews target = new CodeReviews();
            string name01 = "Mario Moreno";
            string name02 = "El coco";
            string name03 = "User 01";
            string name04 = "User 02";
            string name05 = "El Yeti";
            string name06 = "ET";
            
            target.AddProjectMember(name01);
            target.AddProjectMember(name02);
            target.AddProjectMember(name03);
            target.AddProjectMember(name04);
            target.AddProjectMember(name05);
            target.AddProjectMember(name06);

            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name02);
            target.AddCodeReview(name02);
            target.AddCodeReview(name03);
            target.AddCodeReview(name03);
            target.AddCodeReview(name04);
            target.AddCodeReview(name06);
            target.AddCodeReview(name06);
            target.AddCodeReview(name06);

            Dictionary<string, int> actual;
            actual = target.GetTopFive();
            Assert.AreEqual(5, actual.Count, "La cantidad devuelta por el top five deberia se 5");
        }

        /// <summary>
        /// A test for AddProjectMember
        /// </summary>
        [TestMethod()]
        public void AddProjectMemberTest()
        {
            CodeReviews target = new CodeReviews();
            string name = "Mario Moreno";
            target.AddProjectMember(name);
            Assert.AreEqual(0, target.GetCodeReviewsForTeamMember(name), "Deberia haber un team member en la colección.");
        }

        /// <summary>
        /// A test for AddCodeReview
        /// </summary>
        [TestMethod()]
        public void AddCodeReviewTest()
        {
            CodeReviews target = new CodeReviews();
            string name = "Mario Moreno";
            target.AddProjectMember(name);
            target.AddCodeReview(name);
            Assert.AreEqual(1, target.GetCodeReviewsForTeamMember(name), string.Format("'{0}' deberia tener 1 code review asignados.", name));
            target.AddCodeReview(name);
            target.AddCodeReview(name);
            Assert.AreEqual(3, target.GetCodeReviewsForTeamMember(name), string.Format("'{0}' deberia tener 3 code review asignados.", name));
        }

        /// <summary>
        /// A test for GetCodeReviewsForTeamMember
        /// </summary>
        [TestMethod()]
        public void GetCodeReviewsForTeamMemberTest()
        {
            CodeReviews target = new CodeReviews();
            string name = string.Empty;
            int expected = -1; 
            int actual;
            actual = target.GetCodeReviewsForTeamMember(name);
            Assert.AreEqual(expected, actual, "La cantidad de reviews para un team member no existente debería ser 0.");
            name = "Mario Moreno";
            target.AddProjectMember(name);
            target.AddCodeReview(name);
            target.AddCodeReview(name);
            Assert.AreEqual(2, target.GetCodeReviewsForTeamMember(name), string.Format("'{0}' deberia tener 2 code review asignados.", name));
        }

        /// <summary>
        /// A test for GetTeamMembersWithoutCodeReviews
        /// </summary>
        [TestMethod()]
        public void GetTeamMembersWithoutCodeReviewsTest()
        {
            CodeReviews target = new CodeReviews();
            string name01 = "Mario Moreno";
            string name02 = "El coco";
            string name03 = "User 01";
            string name04 = "User 02";
            string name05 = "El Yeti";
            string name06 = "ET";

            target.AddProjectMember(name01);
            target.AddProjectMember(name02);
            target.AddProjectMember(name03);
            target.AddProjectMember(name04);
            target.AddProjectMember(name05);
            target.AddProjectMember(name06);

            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name02);
            target.AddCodeReview(name02);
            target.AddCodeReview(name03);
            target.AddCodeReview(name03);
            target.AddCodeReview(name06);
            target.AddCodeReview(name06);
            target.AddCodeReview(name06);

            List<string> actual;
            actual = target.GetTeamMembersWithoutCodeReviews();
            Assert.AreEqual(2, actual.Count, "La cantidad de team members sin code review debería ser 2");
        }

        /// <summary>
        /// A test for GetTeamMembersWithLessCodeReviews
        /// </summary>
        [TestMethod()]
        public void GetTeamMembersWithLessCodeReviewsTest()
        {
            CodeReviews target = new CodeReviews();
            string name01 = "Mario Moreno";
            string name02 = "El coco";
            string name03 = "User 01";
            string name04 = "User 02";
            string name05 = "El Yeti";
            string name06 = "ET";

            target.AddProjectMember(name01);
            target.AddProjectMember(name02);
            target.AddProjectMember(name03);
            target.AddProjectMember(name04);
            target.AddProjectMember(name05);
            target.AddProjectMember(name06);

            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name01);
            target.AddCodeReview(name02);
            target.AddCodeReview(name02);
            target.AddCodeReview(name03);
            target.AddCodeReview(name06);
            target.AddCodeReview(name06);
            target.AddCodeReview(name06);
            target.AddCodeReview(name04);
            target.AddCodeReview(name05);

            List<string> actual;
            actual = target.GetTeamMembersWithLessCodeReviews();
            Assert.AreEqual(3, actual.Count, "La cantidad de team members con menos code review debería ser 3");
        }
    }
}

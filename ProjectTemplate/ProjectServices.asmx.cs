using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Security.Cryptography;
using System.EnterpriseServices;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Web.Services.Description;

namespace ProjectTemplate
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]

    public class ProjectServices : System.Web.Services.WebService
    {
        ////////////////////////////////////////////////////////////////////////
        ///replace the values of these variables with your database credentials
        ////////////////////////////////////////////////////////////////////////
        private string dbID = "spring2024team2";
        private string dbPass = "spring2024team2";
        private string dbName = "spring2024team2";
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        ///call this method anywhere that you need the connection string!
        ////////////////////////////////////////////////////////////////////////
        private string getConString()
        {
            return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName + "; UID=" + dbID + "; PASSWORD=" + dbPass;
        }
        ////////////////////////////////////////////////////////////////////////


        [WebMethod(EnableSession = true)]
        public int LogOn(string userId, string pass)
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT id, supervisor, issupervisor FROM users WHERE userid=@idValue and pass=@passValue";
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(userId));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            DataTable sqlDt = new DataTable();
            sqlDa.Fill(sqlDt);


            if (sqlDt.Rows.Count > 0)
            {

                Session["id"] = sqlDt.Rows[0]["id"];
                Session["supervisor"] = sqlDt.Rows[0]["supervisor"];
                Session["issupervisor"] = sqlDt.Rows[0]["issupervisor"];
                return Convert.ToInt32(Session["issupervisor"]);
            }

            return -1;
        }

        [WebMethod(EnableSession = true)]
        public bool LogOff()
        {
            Session.Clear();
            Session.Abandon();
            return true;
        }

        [WebMethod(EnableSession = true)]
        public int IsSupervisor()
        {
            if (Session["issupervisor"] != null)
            {
                return Convert.ToInt32(Session["issupervisor"]);
            }
            else
            {
                return -1;
            }
        }

        [WebMethod(EnableSession = true)]
        //this returns a bool of true if the reset took place, which will allow us to display a message either way
        public bool PasswordReset(string userid, string pass, string newpass)
        {
            bool success = false;
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT id FROM users WHERE userid=@useridValue and pass=@passValue; UPDATE users SET pass = @newPassValue WHERE userid=@useridValue and pass=@passValue";
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);


            sqlCommand.Parameters.AddWithValue("@useridValue", HttpUtility.UrlDecode(userid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));
            sqlCommand.Parameters.AddWithValue("@newPassValue", HttpUtility.UrlDecode(newpass));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            DataTable sqlDt = new DataTable();
            sqlDa.Fill(sqlDt);

            if (sqlDt.Rows.Count > 0)
            {


                sqlConnection.Open();
                try
                {
                    sqlCommand.ExecuteNonQuery();
                    success = true;

                }
                catch (Exception e)
                {
                }
                sqlConnection.Close();
            }
            return success;
        }


        [WebMethod(EnableSession = true)]
        public Account UpdateAccount(string pid, string department, string supervisor, string issupervisor)
        {
            DataTable sqlDt = new DataTable("userData");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect;
            if (Convert.ToInt32(issupervisor) == 1)
            {
                sqlSelect = "update users set department=@departmentValue, supervisor=NULL, issupervisor=@issupervisorValue where id=@idValue; SELECT id, userid, pass, department, issupervisor, supervisor from users where id=111137;";
            }
            else
            {
                sqlSelect = "update users set department=@departmentValue, supervisor=@supervisorValue, issupervisor=@issupervisorValue where id=@idValue; SELECT id, userid, pass, department, issupervisor, supervisor from users where id=111137;";
            }
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", Convert.ToInt32(pid));
            sqlCommand.Parameters.AddWithValue("@departmentValue", HttpUtility.UrlDecode(department));
            sqlCommand.Parameters.AddWithValue("@supervisorValue", Convert.ToInt32(supervisor));
            sqlCommand.Parameters.AddWithValue("@issupervisorValue", Convert.ToInt32(issupervisor));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            Account userData = new Account();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                userData.id = Convert.ToInt32(sqlDt.Rows[i]["id"]);
                userData.userId = sqlDt.Rows[i]["userid"].ToString();
                userData.pass = sqlDt.Rows[i]["pass"].ToString();
                userData.department = sqlDt.Rows[i]["department"].ToString();
                userData.issupervisor = sqlDt.Rows[i]["issupervisor"].ToString();
                userData.supervisor = sqlDt.Rows[i]["supervisor"] != DBNull.Value ? Convert.ToInt32(sqlDt.Rows[i]["supervisor"]) : 0;
            }
            return userData;
        }

        //EXAMPLE OF AN INSERT QUERY WITH PARAMS PASSED IN.  BONUS GETTING THE INSERTED ID FROM THE DB!
        [WebMethod(EnableSession = true)]
        public int RequestAccount(string userid, string pass, string department, string issupervisor, string supervisor)
        {
            var newAccountID = -333;
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect;
            if (Convert.ToInt32(issupervisor) == 1)
            {
                sqlSelect = "INSERT into users (userid, pass, department, issupervisor) Values(@uidValue, @passValue, @departmentValue, @issupervisorValue); SELECT LAST_INSERT_ID();";
            }
            else
            {
                sqlSelect = "INSERT into users (userid, pass, department, issupervisor, supervisor) Values(@uidValue, @passValue, @departmentValue, @issupervisorValue, @supervisorValue); SELECT LAST_INSERT_ID();";
            }

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@uidValue", HttpUtility.UrlDecode(userid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));
            sqlCommand.Parameters.AddWithValue("@departmentValue", HttpUtility.UrlDecode(department));
            sqlCommand.Parameters.AddWithValue("@issupervisorValue", HttpUtility.UrlDecode(issupervisor));
            sqlCommand.Parameters.AddWithValue("@supervisorValue", Convert.ToInt32(supervisor));


            sqlConnection.Open();

            try
            {
                newAccountID = Convert.ToInt32(sqlCommand.ExecuteScalar());
            }
            catch (Exception e)
            {
                newAccountID = -1;
            }
            sqlConnection.Close();

            return newAccountID;
        }

        [WebMethod(EnableSession = true)]
        public Account[] GetAccounts()
        {

            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("accounts");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect = "select id, userid, pass,department,supervisor, issupervisor from users where id > 0  order by department";
                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                //gonna use this to fill a data table
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                //filling the data table
                sqlDa.Fill(sqlDt);

                //loop through each row in the dataset, creating instances
                //of our container class Account.  Fill each acciount with
                //data from the rows, then dump them in a list.
                List<Account> accounts = new List<Account>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    //only share user id and pass info with admins!
                    if (Convert.ToInt32(Session["issupervisor"]) == 1)
                    {
                        accounts.Add(new Account
                        {
                            id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                            userId = sqlDt.Rows[i]["userId"].ToString(),
                            pass = sqlDt.Rows[i]["pass"].ToString(),
                            department = sqlDt.Rows[i]["department"].ToString(),
                            supervisor = sqlDt.Rows[i]["supervisor"] != DBNull.Value ? Convert.ToInt32(sqlDt.Rows[i]["supervisor"]) : 0,
                            issupervisor = sqlDt.Rows[i]["issupervisor"].ToString()

                        });
                    }
                    else
                    {
                        accounts.Add(new Account
                        {
                            id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                            userId = sqlDt.Rows[i]["userId"].ToString(),
                            department = sqlDt.Rows[i]["department"].ToString(),
                            issupervisor = sqlDt.Rows[i]["issupervisor"].ToString()
                        });
                    }
                }
                //convert the list of accounts to an array and return!
                return accounts.ToArray();
            }
            else
            {
                return new Account[0];
            }
        }

        [WebMethod(EnableSession = true)]
        public int UnsolicitedFeedback(string problemArea, string complaint, string proposedSolution)
        {
            int unsolicitedFeedbackID = -333;
            string sqlSelect = "insert into unsolicitedFeedback (problemArea, complaint, proposedSolution, submittedBy) " +
                "values(@problemAreaValue, @complaintValue, @proposedSolutionValue, @idValue); SELECT LAST_INSERT_ID();";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@problemAreaValue", HttpUtility.UrlDecode(problemArea));
            sqlCommand.Parameters.AddWithValue("@complaintValue", HttpUtility.UrlDecode(complaint));
            sqlCommand.Parameters.AddWithValue("@proposedSolutionValue", HttpUtility.UrlDecode(proposedSolution));
            sqlCommand.Parameters.AddWithValue("@idValue", Convert.ToInt32(Session["id"]));

            sqlConnection.Open();
            try
            {
                unsolicitedFeedbackID = Convert.ToInt32(sqlCommand.ExecuteScalar());

                //if the query worked, unsolicitedFeedbackID will have the value of the primary key of the
                //row we just inserted into the unsolicitedFeedback table
            }
            catch (Exception e)
            {
                unsolicitedFeedbackID = -1;
                //something went wrong, so we don't have an unsolicitedFeedbackID to work with
                //so we need to set it to something that we can check for later
                //in this case, -1 will (hopefully) never be a valid unsolicitedFeedbackID
                //so we can check for that later when this method is called
            }
            sqlConnection.Close();
            return unsolicitedFeedbackID;
            //return value will be the unsolicitedFeedbackID of the row we just inserted.  If the insert failed,
            //it will be -1 instead.  This value can be used to display the feedback back to the user after
            //they submit it, or to display a message if the submission failed.
        }

        [WebMethod(EnableSession = true)]
        public int SubmitQuestion(string questionText, string daysToLive, string questionType)
        {
            int questionID = -333;
            int daysToLiveInt = 0;
            try
            {
                daysToLiveInt = Convert.ToInt32(daysToLive);
            }
            catch (Exception e)
            {
                daysToLiveInt = 0;
            }
            if (daysToLiveInt <= 0)
            {
                // Default to 99999 days if the user enters a negative number or 0 to signify no expiration
                daysToLiveInt = 99999;
            }

            string sqlSelect = "insert into questions (questionText, expiryDate, submittedBy, questionType) " +
                "values(@questionTextValue, @expiryDateValue, @idValue, @questionType); SELECT LAST_INSERT_ID();";
            DateTime thisDay = DateTime.Today;
            DateTime expiryDate = thisDay.AddDays(daysToLiveInt);
            string expiryDateStr = expiryDate.ToString("yyyy-MM-dd");


            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@questionTextValue", HttpUtility.UrlDecode(questionText));
            sqlCommand.Parameters.AddWithValue("@expiryDateValue", HttpUtility.UrlDecode(expiryDateStr));
            sqlCommand.Parameters.AddWithValue("@questionType", HttpUtility.UrlDecode(questionType));
            sqlCommand.Parameters.AddWithValue("@idValue", Convert.ToInt32(Session["id"]));

            sqlConnection.Open();
            try
            {
                questionID = Convert.ToInt32(sqlCommand.ExecuteScalar());

                //if the query worked, questionID will have the value of the primary key of the
                //row we just inserted into the unsolicitedFeedback table
            }
            catch (Exception e)
            {
                questionID = -1;
                //something went wrong, so we don't have a questionID to work with
                //so we need to set it to something that we can check for later
                //in this case, -1 will (hopefully) never be a valid questionID
                //so we can check for that later when this method is called
            }
            sqlConnection.Close();
            return questionID;
            //return value will be the questionID of the row we just inserted.  If the insert failed,
            //it will be -1 instead.  This value can be used to display the question back to the user after
            //they submit it, or to display a message if the submission failed.
        }

        [WebMethod(EnableSession = true)]
        public int SubmitMultipleChoice(List<string> array)
        {
            int questionID = Convert.ToInt32(array[0]);
            int responseID = -333;
            for (int i = 1; i < array.Count; i++)
            {
                string sqlSelect = "insert into multipleChoice (question, choice) " +
                    "values(@questionValue, @choiceValue); SELECT LAST_INSERT_ID();";

                MySqlConnection sqlConnection = new MySqlConnection(getConString());
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@questionValue", questionID);
                sqlCommand.Parameters.AddWithValue("@choiceValue", HttpUtility.UrlDecode(array[i]));

                sqlConnection.Open();
                try
                {
                    responseID = Convert.ToInt32(sqlCommand.ExecuteScalar());
                }
                catch (Exception e)
                {
                    responseID = -1;
                    //something went wrong, so we don't have a responseID to work with
                    //so we need to set it to something that we can check for later
                    //in this case, -1 will (hopefully) never be a valid responseID
                    //so we can check for that later when this method is called
                }
                sqlConnection.Close();
            }
            return responseID;
        }

        [WebMethod(EnableSession = true)]
        public int SubmitAnswer(string answerText, string questionID)
        {
            int answerID = -333;
            int questionIDInt = Convert.ToInt32(questionID);
            string sqlSelect = "insert into answers (feedback, submittedBy, question) " +
                "values(@feedbackValue, @idValue, @questionValue); SELECT LAST_INSERT_ID();";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@feedbackValue", HttpUtility.UrlDecode(answerText));
            sqlCommand.Parameters.AddWithValue("@idValue", Convert.ToInt32(Session["id"]));
            sqlCommand.Parameters.AddWithValue("@questionValue", questionIDInt);

            sqlConnection.Open();
            try
            {
                answerID = Convert.ToInt32(sqlCommand.ExecuteScalar());

                //if the query worked, answerID will have the value of the primary key of the
                //row we just inserted into the unsolicitedFeedback table
            }
            catch (Exception e)
            {
                answerID = -1;
                //something went wrong, so we don't have a answerID to work with
                //so we need to set it to something that we can check for later
                //in this case, -1 will (hopefully) never be a valid answerID
                //so we can check for that later when this method is called
            }
            sqlConnection.Close();
            return answerID;
            //return value will be the answerID of the row we just inserted.  If the insert failed,
            //it will be -1 instead.  This value can be used to display the answer back to the user after
            //they submit it, or to display a message if the submission failed.
        }

        [WebMethod(EnableSession = true)]
        public Feedback[] GetQuestions()
        {//LOGIC: get all questions for this user and return them!
            if (Convert.ToInt32(Session["issupervisor"]) == 0)
            {
                DataTable sqlDt = new DataTable("questionList");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                //requests just have active set to 0
                string sqlSelect = "select id, questionText, expiryDate from questions where submittedBy=@supervisor and expiryDate > CURDATE() and id not in (select question from answers where submittedBy=@idvalue) order by expiryDate;";

                MySqlConnection sqlConnection = new MySqlConnection(getConString());
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@idvalue", Session["id"]);
                sqlCommand.Parameters.AddWithValue("@supervisor", Session["supervisor"]);

                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                sqlDa.Fill(sqlDt);

                List<Feedback> questionList = new List<Feedback>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    questionList.Add(new Feedback
                    {
                        id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                        question = sqlDt.Rows[i]["questionText"].ToString(),
                        expiryDate = sqlDt.Rows[i]["expiryDate"].ToString(),
                    });
                }
                //convert the list of accounts to an array and return!
                return questionList.ToArray();
            }
            else if (Convert.ToInt32(Session["issupervisor"]) == 1)
            {
                DataTable sqlDt = new DataTable("questionList");
                DateTime thisDay = DateTime.Today;

                string sqlSelect = "SELECT questions.id, questionText, expiryDate, COUNT(reviewed) AS numAnswers, IFNULL(COUNT(reviewed) - SUM(reviewed),0) AS unreviewed FROM questions LEFT JOIN answers ON questions.id = answers.question  where questions.submittedBy=@id GROUP BY questions.id ORDER BY expiryDate;";

                MySqlConnection sqlConnection = new MySqlConnection(getConString());
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@id", Session["id"]);

                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                sqlDa.Fill(sqlDt);

                List<Feedback> questionList = new List<Feedback>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    bool closed = false;
                    DateTime expiryDate = new DateTime(sqlDt.Rows[i].Field<DateTime>("expiryDate").Year, sqlDt.Rows[i].Field<DateTime>("expiryDate").Month, sqlDt.Rows[i].Field<DateTime>("expiryDate").Day);
                    if (expiryDate <= thisDay)
                    {
                        closed = true;
                    }
                    string expiryDateStr = expiryDate.ToString("yyyy-MM-dd");

                    questionList.Add(new Feedback
                    {
                        id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                        question = sqlDt.Rows[i]["questionText"].ToString(),
                        expiryDate = sqlDt.Rows[i]["expiryDate"].ToString(),
                        numAnswers = Convert.ToInt32(sqlDt.Rows[i]["numAnswers"]),
                        unreviewed = Convert.ToInt32(sqlDt.Rows[i]["unreviewed"]),
                        isExpired = closed
                    });
                }
                //convert the list of accounts to an array and return!
                return questionList.ToArray();
            }
            else
            {
                return new Feedback[0];
            }
        }

        [WebMethod(EnableSession = true)]
        public bool CloseQuestion(string questionID)
        { //LOGIC: set the expiry date to today for the question the user selected to close
            int questionIDInt = Convert.ToInt32(questionID);
            bool success = false;
            DataTable sqlDt = new DataTable("question");

            string sqlSelect = "update questions set expiryDate=CURDATE() where id=@idvalue;";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idvalue", questionIDInt);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            sqlConnection.Open();
            try
            {
                sqlCommand.ExecuteNonQuery();
                success = true;
            }
            catch (Exception e)
            {
            }
            sqlConnection.Close();
            return success;
        }

        [WebMethod(EnableSession = true)]
        public Feedback[] GetQuestionToAnswer(string questionID)
        {//LOGIC: get the question the user selected to answer and return it!
            int questionIDInt = Convert.ToInt32(questionID);
            DataTable sqlDt = new DataTable("question");

            string sqlSelect = "select id, questionText, expiryDate, questionType from questions where id=@idvalue;";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idvalue", questionIDInt);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Feedback> question = new List<Feedback>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                question.Add(new Feedback
                {
                    id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                    question = sqlDt.Rows[i]["questionText"].ToString(),
                    expiryDate = sqlDt.Rows[i]["expiryDate"].ToString(),
                    questionType = sqlDt.Rows[i]["questionType"].ToString()
                });
            }
            //convert the list of accounts to an array and return!
            return question.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public string[] GetMultipleChoiceOptions(string questionID)
        {
            int questionIDInt = Convert.ToInt32(questionID);
            DataTable sqlDt = new DataTable("multipleChoice");

            string sqlSelect = "select choice from multipleChoice where question=@idvalue;";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idvalue", questionIDInt);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<string> multipleChoice = new List<string>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                multipleChoice.Add(sqlDt.Rows[i]["choice"].ToString());
            }
            return multipleChoice.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public Feedback[] GetAnswers(string questionID, string seeall)
        {//LOGIC: get all unreviewed answers for a given question and return them!
         // If seeall is 1, then return all answers, reviewed or not
            if (Convert.ToInt32(Session["id"]) != 0)
            {
                int questionIDInt = Convert.ToInt32(questionID);
                int seeallInt = Convert.ToInt32(seeall);
                DataTable sqlDt = new DataTable("answerList");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect = "select id, feedback, reviewed from answers where question=@questionValue and reviewed <> 1 and discarded <> 1;";
                if (seeallInt == 1)
                {
                    sqlSelect = "select id, feedback, reviewed from answers where question=@questionValue and discarded <> 1;";
                }

                MySqlConnection sqlConnection = new MySqlConnection(getConString());
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@questionValue", questionIDInt);

                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                sqlDa.Fill(sqlDt);

                List<Feedback> answerList = new List<Feedback>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    answerList.Add(new Feedback
                    {
                        id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                        answer = sqlDt.Rows[i]["feedback"].ToString(),
                        reviewed = Convert.ToInt32(sqlDt.Rows[i]["reviewed"]),
                    });
                }
                //convert the list of answers to an array and return!
                return answerList.ToArray();
            }
            else
            {
                return new Feedback[0];
            }
        }

        [WebMethod(EnableSession = true)]
        public Feedback[] AnswerReviewed(string questionID, string answerID, string discarded)
        {//LOGIC: set the reviewed flag to 1, discarded flag to 0 or 1 then return remaining answers
            if (Convert.ToInt32(Session["id"]) != 0)
            {
                int questionIDInt = Convert.ToInt32(questionID);
                int answerIDInt = Convert.ToInt32(answerID);
                int discardedInt = Convert.ToInt32(discarded);
                DataTable sqlDt = new DataTable("answerList");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect = "update answers set reviewed=1, reviewedBy=@supervisor, discarded=@discardedValue where id=@answerValue;"
                    + "select id, feedback from answers where question=@questionValue and reviewed <> 1;";

                MySqlConnection sqlConnection = new MySqlConnection(getConString());
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@questionValue", questionIDInt);
                sqlCommand.Parameters.AddWithValue("@answerValue", answerIDInt);
                sqlCommand.Parameters.AddWithValue("@discardedValue", discardedInt);
                sqlCommand.Parameters.AddWithValue("@supervisor", Convert.ToInt32(Session["id"]));

                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                sqlDa.Fill(sqlDt);

                List<Feedback> answerList = new List<Feedback>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    answerList.Add(new Feedback
                    {
                        id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                        answer = sqlDt.Rows[i]["feedback"].ToString(),
                    });
                }
                //convert the list of answers to an array and return!
                return answerList.ToArray();
            }
            else
            {
                return new Feedback[0];
            }
        }

        [WebMethod(EnableSession = true)]
        public List<Feedback> UnsolicitedReviewed(string feedbackID, string discarded)
        {//LOGIC: set the reviewed flag to 1, discarded flag to 0 or 1 then return remaining answers
            if (Convert.ToInt32(Session["id"]) != 0)
            {
                int feedbackIDInt = Convert.ToInt32(feedbackID);
                int discardedInt = Convert.ToInt32(discarded);
                DataTable sqlDt = new DataTable("feedbackList");

                string sqlSelect = "update unsolicitedFeedback set reviewed=1, reviewedBy=@supervisor, discarded=@discardedValue where id=@feedbackValue;"
                    + "SELECT * FROM unsolicitedFeedback WHERE reviewed = 0 and complaint <> 'Placeholder'";

                MySqlConnection sqlConnection = new MySqlConnection(getConString());
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@feedbackValue", feedbackIDInt);
                sqlCommand.Parameters.AddWithValue("@discardedValue", discardedInt);
                sqlCommand.Parameters.AddWithValue("@supervisor", Convert.ToInt32(Session["id"]));

                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                sqlDa.Fill(sqlDt);

                List<Feedback> feedbackList = new List<Feedback>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    feedbackList.Add(new Feedback
                    {
                        id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                        problemArea = sqlDt.Rows[i]["problemArea"].ToString(),
                        complaint = sqlDt.Rows[i]["complaint"].ToString(),
                        suggestion = sqlDt.Rows[i]["proposedSolution"].ToString(),
                    });
                }
                //convert the list of feedback to an array and return!
                return feedbackList;
            }
            else
            {
                return new List<Feedback>();
            }
        }

        // Note: this is for the dropdown menu for the unsolicited feedback
        [WebMethod(EnableSession = true)]
        public List<string> GetProblemAreas()
        {
            List<string> problemAreas = new List<string>();
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT DISTINCT problemArea FROM unsolicitedFeedback WHERE problemArea IS NOT NULL AND problemArea != '' ORDER BY problemArea";
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            try
            {
                sqlConnection.Open();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    problemAreas.Add(reader["problemArea"].ToString());
                }
            }
            catch (Exception e)
            {
                // Handle the exception
            }
            finally
            {
                sqlConnection.Close();
            }
            return problemAreas;
        }

        // Note: this is for the dropdown menu for the supervisor list
        [WebMethod(EnableSession = true)]
        public Account[] GetSupervisorList()
        {
            DataTable sqlDt = new DataTable("supervisorList");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT id, userid FROM users WHERE issupervisor = 1 ORDER BY userid";
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Account> supervisorList = new List<Account>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                supervisorList.Add(new Account
                {
                    id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                    userId = sqlDt.Rows[i]["userid"].ToString(),
                });
            }
            return supervisorList.ToArray();
        }

        // Note: this is for the dropdown menu for the user list
        [WebMethod(EnableSession = true)]
        public Account[] GetUserList()
        {
            DataTable sqlDt = new DataTable("userList");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT id, userid FROM users ORDER BY userid";
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Account> userList = new List<Account>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                userList.Add(new Account
                {
                    id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                    userId = sqlDt.Rows[i]["userid"].ToString(),
                });
            }
            return userList.ToArray();
        }

        // Note: this is to populate a user for editing
        [WebMethod(EnableSession = true)]
        public Account GetUser(string id)
        {
            DataTable sqlDt = new DataTable("userData");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT id, userid, pass, department, issupervisor, supervisor FROM users where id = @idValue";
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", Convert.ToInt32(id));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            Account userData = new Account();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                userData.id = Convert.ToInt32(sqlDt.Rows[i]["id"]);
                userData.userId = sqlDt.Rows[i]["userid"].ToString();
                userData.pass = sqlDt.Rows[i]["pass"].ToString();
                userData.department = sqlDt.Rows[i]["department"].ToString();
                userData.issupervisor = sqlDt.Rows[i]["issupervisor"].ToString();
                userData.supervisor = sqlDt.Rows[i]["supervisor"] != DBNull.Value ? Convert.ToInt32(sqlDt.Rows[i]["supervisor"]) : 0;
            }
            return userData;
        }

        // New web method for Unsolicited Feedback
        [WebMethod(EnableSession = true)]
        public List<Feedback> GetUnsolicitedFeedback()
        {
            // This checks if the session indicates the user is a supervisor
            if (Session["issupervisor"] != null && Convert.ToInt32(Session["issupervisor"]) == 1)
            {
                List<Feedback> feedbackList = new List<Feedback>();
                string sqlConnectString = ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect = "SELECT * FROM unsolicitedFeedback WHERE reviewed = 0 and complaint <> 'Placeholder'";

                using (MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString))
                {
                    MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
                    sqlConnection.Open();
                    MySqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        Feedback feedback = new Feedback
                        {
                            id = Convert.ToInt32(reader["id"]),
                            problemArea = reader["problemArea"].ToString(),
                            complaint = reader["complaint"].ToString(),
                            suggestion = reader["proposedSolution"].ToString(),
                        };
                        feedbackList.Add(feedback);
                    }
                    sqlConnection.Close();
                }
                return feedbackList;
            }
            else
            {
                // Return empty list or handle accordingly if the user is not a supervisor
                return new List<Feedback>();
            }
        }

        [WebMethod(EnableSession = true)]
        public Activity[] GetMostActiveUsers(string reportType, string department)
        {
            DataTable sqlDt = new DataTable("activity");

            string sqlSelect;
            if (reportType == "answers")
            {
                if (department == "mine")
                {
                    sqlSelect = "SELECT users.id, userid, department, COUNT(submittedBy) AS numAnswers FROM users LEFT JOIN answers ON users.id = answers.submittedBy WHERE department = (SELECT department FROM users WHERE id = @idValue) AND discarded = 0 GROUP BY users.id HAVING COUNT(submittedBy) > 0 ORDER BY numAnswers DESC LIMIT 3;";
                }
                else if (department == "all")
                {
                    sqlSelect = "SELECT users.id, userid, department, COUNT(submittedBy) AS numAnswers FROM users LEFT JOIN answers ON users.id = answers.submittedBy WHERE discarded = 0 GROUP BY users.id HAVING COUNT(submittedBy) > 0 ORDER BY numAnswers DESC LIMIT 3;";
                }
                else
                {
                    sqlSelect = "SELECT users.id, userid, department, COUNT(submittedBy) AS numAnswers FROM users LEFT JOIN answers ON users.id = answers.submittedBy WHERE department = @departmentValue AND discarded = 0 GROUP BY users.id HAVING COUNT(submittedBy) > 0 ORDER BY numAnswers DESC LIMIT 3;";
                }
            }
            else if (reportType == "unsolicitedFeedback")
            {
                if (department == "mine")
                {
                    sqlSelect = "SELECT users.id, userid, department, COUNT(submittedBy) AS numUSF FROM users LEFT JOIN unsolicitedFeedback ON users.id = unsolicitedFeedback.submittedBy WHERE department = (SELECT department FROM users WHERE id = @idValue) AND discarded = 0 GROUP BY users.id HAVING COUNT(submittedBy) > 0 ORDER BY numUSF DESC LIMIT 3;";
                }
                else if (department == "all")
                {
                    sqlSelect = "SELECT users.id, userid, department, COUNT(submittedBy) AS numUSF FROM users LEFT JOIN unsolicitedFeedback ON users.id = unsolicitedFeedback.submittedBy WHERE discarded = 0 GROUP BY users.id HAVING COUNT(submittedBy) > 0 ORDER BY numUSF DESC LIMIT 3;";
                }
                else
                {
                    sqlSelect = "SELECT users.id, userid, department, COUNT(submittedBy) AS numUSF FROM users LEFT JOIN unsolicitedFeedback ON users.id = unsolicitedFeedback.submittedBy WHERE department = @departmentValue AND discarded = 0 GROUP BY users.id HAVING COUNT(submittedBy) > 0 ORDER BY numUSF DESC LIMIT 3;";
                }
            }
            else
            {
                if (department == "mine")
                {
                    sqlSelect = "SELECT users.id AS id, users.userid AS userid, department, IFNULL(numAnswers,0) AS numAnswers, IFNULL(numUSF,0) AS numUSF, (IFNULL(numAnswers, 0) + IFNULL(numUSF,0)) AS numTotal FROM users LEFT JOIN (SELECT users.id as idNum, userid, COUNT(submittedBy) AS numAnswers FROM users JOIN answers ON users.id = answers.submittedBy WHERE department = (SELECT department FROM users WHERE id = @idValue) AND discarded = 0 GROUP BY users.id ORDER BY numAnswers DESC) as AnswersList ON users.id = AnswersList.idNum LEFT JOIN (SELECT users.id as idNum, userid, COUNT(submittedBy) AS numUSF FROM users JOIN unsolicitedFeedback ON users.id = unsolicitedFeedback.submittedBy WHERE department = (SELECT department FROM users WHERE id = @idValue) AND discarded = 0 GROUP BY users.id ORDER BY numUSF DESC) AS USFList ON users.id = USFList.idNum WHERE numAnswers > 0 or numUSF > 0 ORDER BY numAnswers + numUSF DESC LIMIT 3;";
                }
                else if (department == "all")
                {
                    sqlSelect = "SELECT users.id AS id, users.userid AS userid, department, IFNULL(numAnswers,0) AS numAnswers, IFNULL(numUSF,0) AS numUSF, (IFNULL(numAnswers, 0) + IFNULL(numUSF,0)) AS numTotal FROM users LEFT JOIN (SELECT users.id as idNum, userid, COUNT(submittedBy) AS numAnswers FROM users JOIN answers ON users.id = answers.submittedBy WHERE discarded = 0 GROUP BY users.id ORDER BY numAnswers DESC) as AnswersList ON users.id = AnswersList.idNum LEFT JOIN (SELECT users.id as idNum, userid, COUNT(submittedBy) AS numUSF FROM users JOIN unsolicitedFeedback ON users.id = unsolicitedFeedback.submittedBy WHERE discarded = 0 GROUP BY users.id ORDER BY numUSF DESC) AS USFList ON users.id = USFList.idNum WHERE numAnswers > 0 or numUSF > 0 ORDER BY numTotal DESC LIMIT 3;";
                }
                else
                {
                    sqlSelect = "SELECT users.id AS id, users.userid AS userid, department, IFNULL(numAnswers,0) AS numAnswers, IFNULL(numUSF,0) AS numUSF, (IFNULL(numAnswers, 0) + IFNULL(numUSF,0)) AS numTotal FROM users LEFT JOIN (SELECT users.id as idNum, userid, COUNT(submittedBy) AS numAnswers FROM users JOIN answers ON users.id = answers.submittedBy WHERE department = @departmentValue AND discarded = 0 GROUP BY users.id ORDER BY numAnswers DESC) as AnswersList ON users.id = AnswersList.idNum LEFT JOIN (SELECT users.id as idNum, userid, COUNT(submittedBy) AS numUSF FROM users JOIN unsolicitedFeedback ON users.id = unsolicitedFeedback.submittedBy WHERE department = @departmentValue AND discarded = 0 GROUP BY users.id ORDER BY numUSF DESC) AS USFList ON users.id = USFList.idNum WHERE numAnswers > 0 or numUSF > 0 ORDER BY numAnswers + numUSF DESC LIMIT 3;";
                }
            }

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", Convert.ToInt32(Session["id"]));
            sqlCommand.Parameters.AddWithValue("@departmentValue", HttpUtility.UrlDecode(department));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Activity> activityList = new List<Activity>();
            if (reportType == "answers")
            {
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    activityList.Add(new Activity
                    {
                        id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                        userId = sqlDt.Rows[i]["userid"].ToString(),
                        department = sqlDt.Rows[i]["department"].ToString(),
                        numAnswers = Convert.ToInt32(sqlDt.Rows[i]["numAnswers"]),
                        numUSF = 0,
                        numTotal = 0
                    });
                }
            }
            else if (reportType == "unsolicitedFeedback")
            {
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    activityList.Add(new Activity
                    {
                        id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                        userId = sqlDt.Rows[i]["userid"].ToString(),
                        department = sqlDt.Rows[i]["department"].ToString(),
                        numAnswers = 0,
                        numUSF = Convert.ToInt32(sqlDt.Rows[i]["numUSF"]),
                        numTotal = 0
                    });
                }
            }
            else
            {
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    activityList.Add(new Activity
                    {
                        id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                        userId = sqlDt.Rows[i]["userid"].ToString(),
                        department = sqlDt.Rows[i]["department"].ToString(),
                        numAnswers = Convert.ToInt32(sqlDt.Rows[i]["numAnswers"]),
                        numUSF = Convert.ToInt32(sqlDt.Rows[i]["numUSF"]),
                        numTotal = Convert.ToInt32(sqlDt.Rows[i]["numTotal"])
                    });
                }
            }
            return activityList.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public Activity GetMyActivity()
        {
            DataTable sqlDt = new DataTable("activity");

            string sqlSelect = "SELECT users.id AS id, users.userid AS userid, IFNULL(numAnswers, 0) AS numAnswers, IFNULL(numUSF, 0) AS numUSF, (IFNULL(numAnswers, 0) + IFNULL(numUSF, 0)) AS numTotal FROM users LEFT JOIN(SELECT users.id as idNum, userid, COUNT(submittedBy) AS numAnswers FROM users LEFT JOIN answers ON users.id = answers.submittedBy WHERE users.id = @idValue AND discarded = 0 GROUP BY users.id) as AnswersList ON users.id = AnswersList.idNum LEFT JOIN(SELECT users.id as idNum, userid, COUNT(submittedBy) AS numUSF FROM users LEFT JOIN unsolicitedFeedback ON users.id = unsolicitedFeedback.submittedBy WHERE users.id = @idValue AND discarded = 0 GROUP BY users.id) AS USFList ON users.id = USFList.idNum WHERE users.id = @idValue;";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", Convert.ToInt32(Session["id"]));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            Activity myActivity = new Activity
            {
                id = Convert.ToInt32(sqlDt.Rows[0]["id"]),
                userId = sqlDt.Rows[0]["userid"].ToString(),
                numAnswers = Convert.ToInt32(sqlDt.Rows[0]["numAnswers"]),
                numUSF = Convert.ToInt32(sqlDt.Rows[0]["numUSF"]),
                numTotal = Convert.ToInt32(sqlDt.Rows[0]["numTotal"])
            };
            return myActivity;
        }

        // Note: this is for the dropdown menu for selecting different departments
        [WebMethod(EnableSession = true)]
        public List<string> GetDepartments()
        {
            List<string> departments = new List<string>();
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT users.department, AnswerList.numAnswers, USFList.numUSF FROM users LEFT JOIN(SELECT department, IFNULL(COUNT(answers.id), 0) as numAnswers FROM users JOIN answers ON users.id = answers.submittedBy GROUP BY department) as AnswerList ON users.department = AnswerList.department LEFT JOIN(SELECT department, IFNULL(COUNT(unsolicitedFeedback.id),0) as numUSF FROM users JOIN unsolicitedFeedback ON users.id = unsolicitedFeedback.submittedBy GROUP BY department) as USFList ON users.department = USFList.department GROUP BY department HAVING numAnswers > 0 OR numUSF > 0";
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            try
            {
                sqlConnection.Open();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    departments.Add(reader["department"].ToString());
                }
            }
            catch (Exception e)
            {
                // Handle the exception
            }
            finally
            {
                sqlConnection.Close();
            }
            return departments;
        }
    }
}

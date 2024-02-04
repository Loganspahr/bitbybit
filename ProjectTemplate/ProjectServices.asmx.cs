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


        //EXAMPLE OF AN INSERT QUERY WITH PARAMS PASSED IN.  BONUS GETTING THE INSERTED ID FROM THE DB!
        [WebMethod(EnableSession = true)]
        public bool RequestAccount(string pid, string userid, string pass, string department, string issupervisor)
        {
            bool success = false;
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "INSERT into users (id, userid, pass, department, issupervisor) Values(@idValue, @uidValue, @passValue, @departmentValue, @issupervisorValue)";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(pid));
            sqlCommand.Parameters.AddWithValue("@uidValue", HttpUtility.UrlDecode(userid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));
            sqlCommand.Parameters.AddWithValue("@departmentValue", HttpUtility.UrlDecode(department));
            sqlCommand.Parameters.AddWithValue("@issupervisorValue", HttpUtility.UrlDecode(issupervisor));


            sqlConnection.Open();

            try
            {
                sqlCommand.ExecuteScalar();
                success = true;
            }
            catch (Exception e)
            {
            }
            sqlConnection.Close();

            return success;
        }

        [WebMethod(EnableSession = true)]
        public Account[] GetAccounts()
        {

            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("accounts");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect = "select id, userid, pass,department,supervisor, issupervisor from users where id > 0  order by department";
                // note - i need to add a line possibly in this code to check the accountlocked, where accountlocked < 3 and hang the code above
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
        public int SubmitQuestion(string questionText, string daysToLive)
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

            string sqlSelect = "insert into questions (questionText, expiryDate, submittedBy) " +
                "values(@questionTextValue, @expiryDateValue, @idValue); SELECT LAST_INSERT_ID();";
            DateTime thisDay = DateTime.Today;
            DateTime expiryDate = thisDay.AddDays(daysToLiveInt);
            string expiryDateStr = expiryDate.ToString("yyyy-MM-dd");


            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@questionTextValue", HttpUtility.UrlDecode(questionText));
            sqlCommand.Parameters.AddWithValue("@expiryDateValue", HttpUtility.UrlDecode(expiryDateStr));
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
        public int SubmitAnswer(string userID, string answerText, string questionID)
        {
            int answerID = -333;
            int questionIDInt = Convert.ToInt32(questionID);
            string sqlSelect = "insert into answers (feedback, submittedBy, question) " +
                "values(@feedbackValue, @idValue, @questionValue); SELECT LAST_INSERT_ID();";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@feedbackValue", HttpUtility.UrlDecode(answerText));
            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(userID));
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
            // This sets session variables for testing as a normal employee
            if (Convert.ToInt32(Session["issupervisor"]) == 0)
            {
                DataTable sqlDt = new DataTable("questionList");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                //requests just have active set to 0
                string sqlSelect = "select id, questionText, expiryDate from questions where submittedBy=@supervisor and expiryDate > CURDATE() order by expiryDate";

                MySqlConnection sqlConnection = new MySqlConnection(getConString());
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

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

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                //requests just have active set to 0
                string sqlSelect = "select id, questionText, expiryDate from questions where submittedBy=@id order by expiryDate";

                MySqlConnection sqlConnection = new MySqlConnection(getConString());
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@id", Session["id"]);

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
            else
            {
                return new Feedback[0];
            }
        }

        [WebMethod(EnableSession = true)]
        public Feedback[] GetAnswers(string questionID)
        {//LOGIC: get all answers for a given question and return them!
            // This sets session variables for testing as a supervisor
            Session["id"] = 5;
            Session["issupervisor"] = 1;
            if (Convert.ToInt32(Session["id"]) != 0)
            {
                int questionIDInt = Convert.ToInt32(questionID);
                DataTable sqlDt = new DataTable("answerList");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                //requests just have active set to 0
                string sqlSelect = "select id, feedback from answers where question=@questionValue and reviewed <> 1";

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
                    });
                }
                //convert the list of accounts to an array and return!
                return answerList.ToArray();
            }
            else
            {
                return new Feedback[0];
            }
        }

        // NOTE: THIS IS ONLY HERE FOR LAZY DEVS TO ONE-CLICK SIGN IN - NEEDS TO BE REMOVED FROM FINAL CODE
        [WebMethod(EnableSession = true)]
        public int Login(string role)
        {
            if (role == "supervisor")
            {
                Session["id"] = 5;
                Session["issupervisor"] = 1;
                return Convert.ToInt32(Session["issupervisor"]);
            }
            else if (role == "employee")
            {
                Session["id"] = 1;
                Session["issupervisor"] = 0;
                Session["supervisor"] = 5;
                return Convert.ToInt32(Session["issupervisor"]);
            }
            else
            {
                return -1;
            }
        }
    }
}

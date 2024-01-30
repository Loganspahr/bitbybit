using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

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
		private string getConString() {
			return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName+"; UID=" + dbID + "; PASSWORD=" + dbPass;
		}
		////////////////////////////////////////////////////////////////////////



		/////////////////////////////////////////////////////////////////////////
		//don't forget to include this decoration above each method that you want
		//to be exposed as a web service!
		[WebMethod(EnableSession = true)]
		/////////////////////////////////////////////////////////////////////////
		public string TestConnection()
		{
			try
			{
				string testQuery = "select * from test";

				////////////////////////////////////////////////////////////////////////
				///here's an example of using the getConString method!
				////////////////////////////////////////////////////////////////////////
				MySqlConnection con = new MySqlConnection(getConString());
				////////////////////////////////////////////////////////////////////////

				MySqlCommand cmd = new MySqlCommand(testQuery, con);
				MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
				DataTable table = new DataTable();
				adapter.Fill(table);
				return "Success!";
			}
			catch (Exception e)
			{
				return "Something went wrong, please check your credentials and db name and try again.  Error: "+e.Message;
			}
		}

        [WebMethod(EnableSession = true)]
        public int UnsolicitedFeedback(string userID, string problemArea, string complaint, string proposedSolution)
        {
			int unsolicitedFeedbackID = -333;
            string sqlSelect = "insert into unsolicitedFeedback (problemArea, complaint, proposedSolution, submittedBy) " +
                "values(@problemAreaValue, @complaintValue, @proposedSolutionValue, @idValue); SELECT LAST_INSERT_ID();";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@problemAreaValue", HttpUtility.UrlDecode(problemArea));
            sqlCommand.Parameters.AddWithValue("@complaintValue", HttpUtility.UrlDecode(complaint));
            sqlCommand.Parameters.AddWithValue("@proposedSolutionValue", HttpUtility.UrlDecode(proposedSolution));
            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(userID));

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
        public int SubmitQuestion(string userID, string questionText, string daysToLive)
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
            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(userID));

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
    }
}

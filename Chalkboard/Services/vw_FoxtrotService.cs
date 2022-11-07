﻿using ChalkboardAPI.Helpers;
using ChalkboardAPI.Models;
using ESCHOOL.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ESCHOOL.Services
{

    public interface Ivw_FoxtrotService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        IEnumerable<vw_Foxtrot> GetAll();
        List< vw_Foxtrot> GetById(string id);
    }
    public class vw_FoxtrotService: Ivw_FoxtrotService
    {
        private List<vw_Foxtrot> _students = new List<vw_Foxtrot>
        {
            new vw_Foxtrot {  ExamName = "test",StudentId="1",
                SubjectId="1", SubjectName = "User", GPA = "test", ResultMarks = "test"
                 }
        };

        private readonly AppSettings _appSettings;
        private readonly IConfiguration _configuration;


        public vw_FoxtrotService(IOptions<AppSettings> appSettings, IConfiguration configuration)
        {
            _appSettings = appSettings.Value;
            _configuration = configuration;

        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _students.SingleOrDefault(x => x.SubjectId == model.Email && x.SubjectName == model.Password);

            // return null if user not found
            if (user == null) return null;

            // authentication successful so generate jwt token
            var token = generateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        public IEnumerable<vw_Foxtrot> GetAll()
        {

            List<vw_Foxtrot> studentProfileViews = new List<vw_Foxtrot>();

            string connectionString = _configuration.GetConnectionString("StudentDB");
            SqlConnection connection = new SqlConnection(connectionString);
            string query = "Select * FROM vw_Foxtrot";
            SqlCommand com = new SqlCommand(query, connection);
            connection.Open();
            SqlDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                vw_Foxtrot studentProfileView = new vw_Foxtrot();
                
                studentProfileView.ExamName = reader["ExamName"].ToString();
                studentProfileView.StudentId = reader["StudentId"].ToString();
                studentProfileView.SubjectId = reader["SubjectId"].ToString();
               
                studentProfileView.SubjectName = reader["SubjectName"].ToString();
                studentProfileView.GPA = reader["GPA"].ToString();
                studentProfileView.ResultMarks = reader["ResultMarks"].ToString();
                
                studentProfileViews.Add(studentProfileView);
            }
            reader.Close();
            connection.Close();
            return studentProfileViews;

            //return _students;
        }

        public List< vw_Foxtrot> GetById(string id)
        {

            List<vw_Foxtrot> stdAttendances = new List<vw_Foxtrot>();
            string connectionString = _configuration.GetConnectionString("StudentDB");
            SqlConnection connection = new SqlConnection(connectionString);
            string query = "Select * FROM vw_Foxtrot where StudentId=" + id + "";
            SqlCommand com = new SqlCommand(query, connection);
            connection.Open();
            SqlDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                vw_Foxtrot stdAttendance = new vw_Foxtrot();
                stdAttendance.ExamName = reader["ExamName"].ToString();
                stdAttendance.StudentId = reader["StudentId"].ToString();
                stdAttendance.SubjectId = reader["SubjectId"].ToString();

                stdAttendance.SubjectName = reader["SubjectName"].ToString();
                stdAttendance.GPA = reader["GPA"].ToString();
                stdAttendance.ResultMarks = reader["ResultMarks"].ToString();
                stdAttendances.Add(stdAttendance);

            }
            return stdAttendances;
            //return _students.FirstOrDefault(x => x.SubjectId == id);
        }

        // helper methods

        private string generateJwtToken(vw_Foxtrot user)
        {
            // generate token that is valid for 3 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("SubjectId", user.SubjectId.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}

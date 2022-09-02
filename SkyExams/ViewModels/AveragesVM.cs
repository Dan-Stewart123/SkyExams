using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SkyExams.ViewModels;

namespace SkyExams.ViewModels
{
    public class AveragesVM
    {
        public ICollection<ExamAverageVM> examAverages { get; set; }

        public string[] getNames()
        {
            string[] names = new string[100];
            for(int temp = 0; temp < examAverages.Count(); temp++)
            {
                names[temp] = examAverages.ElementAt(temp).examName;
            }// for each

            return names;
        }// get names

        public int[] getAvgs()
        {
            int[] avgs = new int[100];
            for (int temp = 0; temp < examAverages.Count(); temp++)
            {
                avgs[temp] = examAverages.ElementAt(temp).examAvg;
            }// for each

            return avgs;
        }// get avgs

        public List<string> getListNames()
        {
            List<string> names = new List<string>();
            foreach(var e in examAverages)
            {
                string temp = e.examName;
                names.Add(temp);
            }// for each

            return names;
        }// get list names

        public List<int> getListAvgs()
        {
            List<int> avgs = new List<int>();
            foreach (var e in examAverages)
            {
                int temp = e.examAvg;
                avgs.Add(temp);
            }// for each

            return avgs;
        }// get list names
    }
}
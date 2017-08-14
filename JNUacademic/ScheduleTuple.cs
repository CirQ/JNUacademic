namespace JNUacademic {
    class ScheduleTuple {
        public string id;
        public string title;
        public string limit;
        public string selected;
        public string instructor;
        public string major;
        public string credit;
        public string location;
        public string time;
        public string classid;
        public string orientation;
        public string exam;
        public string extra;
        public string method;

        public ScheduleTuple(string s0, string s1, string s2, string s3, string s4, string s5, 
            string s6, string s7, string s8, string s9, string sa, string sb, string sc, string sd) {
            id = s0; title = s1; limit = s2; selected = s3; instructor = s4; major = s5; credit = s6;
            location = s7; time = s8; classid = s9; orientation = sa; exam = sb; extra = sc; method = sd;
        }

    }
}

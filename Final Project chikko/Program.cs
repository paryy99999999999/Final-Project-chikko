using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SmartStudyPlanner
{
    // ====================================================
    // 1. DATA MODELS & STRUCTS
    // ====================================================
    class ScheduleSlot
    {
        public string SubjectCode { get; set; }
        public string SectionInfo { get; set; }
        public string RoomInfo { get; set; }
        public int StartMinutes { get; set; }
        public int EndMinutes { get; set; }

        public ScheduleSlot(string subjectCode, string sectionInfo, string roomInfo, int startMinutes, int endMinutes)
        {
            SubjectCode = subjectCode;
            SectionInfo = sectionInfo;
            RoomInfo = roomInfo;
            StartMinutes = startMinutes;
            EndMinutes = endMinutes;
        }

        public string GetFormattedTimeRange()
        {
            int sH = StartMinutes / 60;
            int sM = StartMinutes % 60;
            int eH = EndMinutes / 60;
            int eM = EndMinutes % 60;
            return $"{sH:D2}.{sM:D2}-{eH:D2}.{eM:D2}";
        }
    }

    class GradeScheme
    {
        public double A { get; set; } = 80.0;
        public double BPlus { get; set; } = 75.0;
        public double B { get; set; } = 70.0;
        public double CPlus { get; set; } = 65.0;
        public double C { get; set; } = 60.0;
        public double DPlus { get; set; } = 55.0;
        public double D { get; set; } = 50.0;

        public string GetGrade(double score)
        {
            if (score >= A) return "A";
            if (score >= BPlus) return "B+";
            if (score >= B) return "B";
            if (score >= CPlus) return "C+";
            if (score >= C) return "C";
            if (score >= DPlus) return "D+";
            if (score >= D) return "D";
            return "F";
        }

        public double GetNextGradeTarget(double score, out string nextGrade)
        {
            if (score < D) { nextGrade = "D"; return D; }
            if (score < DPlus) { nextGrade = "D+"; return DPlus; }
            if (score < C) { nextGrade = "C"; return C; }
            if (score < CPlus) { nextGrade = "C+"; return CPlus; }
            if (score < B) { nextGrade = "B"; return B; }
            if (score < BPlus) { nextGrade = "B+"; return BPlus; }
            { nextGrade = "A"; return A; }
        }
    }

    class Subject
    {
        public string Name { get; set; }
        public GradeScheme Scheme { get; set; }
        public List<ScoreItem> Criteria { get; set; }

        public Subject(string name, GradeScheme scheme)
        {
            Name = name;
            Scheme = scheme;
            Criteria = new List<ScoreItem>();
        }

        public double CalculateTotalScore100()
        {
            double totalPercent = 0;
            double totalRawEarned = 0;
            double totalRawMax = 0;

            foreach (var item in Criteria)
            {
                if (!item.IsEntered) continue;

                if (item.IsPercentage)
                {
                    if (item.MaxScore > 0)
                    {
                        totalPercent += (item.EarnedScore / item.MaxScore) * item.WeightPercentage;
                    }
                }
                else
                {
                    totalRawEarned += item.EarnedScore;
                    totalRawMax += item.MaxScore;
                }
            }

            double rawPercentageScore = totalRawMax > 0 ? (totalRawEarned / totalRawMax) * 100 : 0;
            return totalPercent + rawPercentageScore;
        }
    }

    class ScoreItem
    {
        public string ItemName { get; set; }
        public double MaxScore { get; set; }
        public double WeightPercentage { get; set; }
        public double EarnedScore { get; set; }
        public bool IsPercentage { get; set; }
        public bool IsEntered { get; set; }

        public ScoreItem(string itemName, double maxScore, double weightPercentage, bool isPercentage)
        {
            ItemName = itemName;
            MaxScore = maxScore;
            WeightPercentage = weightPercentage;
            EarnedScore = 0;
            IsPercentage = isPercentage;
            IsEntered = false;
        }
    }

    // ====================================================
    // 2. MAIN PROGRAM
    // ====================================================
    class Program
    {
        static List<Subject> subjects = new List<Subject>();
        static List<ScheduleSlot>[] weeklySchedule = new List<ScheduleSlot>[7];
        static string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "study_planner_data.txt");

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            for (int i = 0; i < 7; i++) weeklySchedule[i] = new List<ScheduleSlot>();

            InitializeDefaultSubjects();
            LoadDataFromFile();

            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine("=========================================================================================");
                Console.WriteLine("         Smart Study & Exam Schedule Planner (Smart Planner)      ");
                Console.WriteLine("=========================================================================================");
                Console.WriteLine(" 1. View & Manage Weekly Timetable");
                Console.WriteLine(" 2. Record Continuous Assessment Scores & Calculate Grades");
                Console.WriteLine(" 3. Add New Subject");
                Console.WriteLine(" 4. Calculate Missing Points Needed for Grade Upgrade");
                Console.WriteLine(" 5. Prioritize Subjects to Focus on for Studying");
                Console.WriteLine(" 6. View Comprehensive Summary for Each Subject");
                Console.WriteLine(" 7. Clear Data");
                Console.WriteLine(" 8. Save Data to File");
                Console.WriteLine(" 9. Exit Program");
                Console.WriteLine("=========================================================================================");
                Console.Write("Select menu (1-9): ");

                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        ManageWeeklySchedule();
                        break;
                    case "2":
                        InputScoresAndCalculateGrade();
                        break;
                    case "3":
                        AddNewCustomSubject();
                        break;
                    case "4":
                        ShowPointsNeededForNextGrade();
                        break;
                    case "5":
                        ShowStudyPriority();
                        break;
                    case "6":
                        ShowComprehensiveSummary();
                        break;
                    case "7":
                        ResetDataMenu();
                        break;
                    case "8":
                        SaveDataToFile();
                        Console.WriteLine("\n✅ [Data successfully saved to file! The latest data has overwritten the existing file]");
                        Console.WriteLine("Press Enter to return...");
                        Console.ReadLine();
                        break;
                    case "9":
                        running = false;
                        Console.WriteLine("\nThank you for using the system!");
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n⚠️ nvalid option! Please choose between 1 and 9 only (Press Enter to try again)");
                        Console.ResetColor();
                        Console.ReadLine();
                        break;
                }
            }
        }

        // ====================================================
        // HELPER: ตรวจสอบคำตอบ yes / no (บังคับพิมพ์ yes หรือ no เท่านั้น)
        // ====================================================
        static bool GetYesNoInput(string promptMessage)
        {
            while (true)
            {
                Console.Write(promptMessage);
                string input = Console.ReadLine()?.Trim().ToLower();

                if (input == "yes") return true;
                if (input == "no") return false;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️ Invalid input! Please type only 'yes' or 'no'");
                Console.ResetColor();
            }
        }

        // ====================================================
        // HELPER: รับค่าเวลาอิสระ (เช่น 10.15, 13.45) รองรับ Enter กลับเมนูหลัก
        // ====================================================
        static int? GetFlexibleTimeInput(string promptMessage)
        {
            while (true)
            {
                Console.Write(promptMessage);
                string inputStr = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(inputStr))
                {
                    return null;
                }

                if (!inputStr.Contains("."))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("⚠️ Invalid time format! You must use a dot '.' to separate hours and minutes, e.g., 10.15 or 13.45");
                    Console.WriteLine("   -> Please try again or [press Enter to return to the main menu]");
                    Console.ResetColor();
                    continue;
                }

                string[] parts = inputStr.Split('.');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int hour) &&
                    int.TryParse(parts[1], out int minute) &&
                    parts[1].Length == 2)
                {
                    if (hour >= 0 && hour <= 23 && minute >= 0 && minute <= 59)
                    {
                        return (hour * 60) + minute;
                    }
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️ Invalid time! Hours must be between 00-23 and minutes 00-59 (e.g., 08.30, 10.45)");
                Console.WriteLine("   -> Please try again or [press Enter to return to the main menu]");
                Console.ResetColor();
            }
        }

        // ====================================================
        // HELPER: รับตัวเลขทศนิยม รองรับ Enter กลับเมนูหลัก
        // ====================================================
        static double? GetValidDoubleInput(string promptMessage, double minVal, double maxVal)
        {
            while (true)
            {
                Console.Write(promptMessage);
                string inputStr = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(inputStr))
                {
                    return null;
                }

                if (double.TryParse(inputStr, out double val) && val >= minVal && val <= maxVal)
                {
                    return val;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"⚠️ ผู้ใช้กรอกเลขไม่ถูกต้อง! ต้องเป็นตัวเลขระหว่าง {minVal} ถึง {maxVal}");
                Console.WriteLine("   -> กรุณาพิมพ์ใหม่ หรือ [กด Enter เปล่าๆ เพื่อกลับหน้าเมนูหลัก]");
                Console.ResetColor();
            }
        }

        // ====================================================
        // HELPER: รับตัวเลขจำนวนเต็ม รองรับ Enter กลับเมนูหลัก
        // ====================================================
        static int? GetValidIntInput(string promptMessage, int minVal, int maxVal)
        {
            while (true)
            {
                Console.Write(promptMessage);
                string inputStr = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(inputStr))
                {
                    return null;
                }

                if (int.TryParse(inputStr, out int val) && val >= minVal && val <= maxVal)
                {
                    return val;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"⚠️ ผู้ใช้กรอกเลขไม่ถูกต้อง! ต้องเป็นตัวเลขระหว่าง {minVal} ถึง {maxVal}");
                Console.WriteLine("   -> กรุณาพิมพ์ใหม่ หรือ [กด Enter เปล่าๆ เพื่อกลับหน้าเมนูหลัก]");
                Console.ResetColor();
            }
        }

        // ====================================================
        // กำหนดข้อมูลทั้ง 5 วิชาเริ่มต้น (คะแนนเต็มจริง + น้ำหนัก %)
        // ====================================================
        static void InitializeDefaultSubjects()
        {
            subjects.Clear();

            // 1. Computer Programming (A >= 80)
            Subject sub1 = new Subject("Computer Programming", new GradeScheme());
            sub1.Criteria.Add(new ScoreItem("Midterm", 90, 30, true));
            sub1.Criteria.Add(new ScoreItem("Final", 90, 30, true));
            sub1.Criteria.Add(new ScoreItem("Group Project", 25, 25, true));
            sub1.Criteria.Add(new ScoreItem("Lab Assignment", 10, 10, true));
            sub1.Criteria.Add(new ScoreItem("Class Participation", 5, 5, true));
            subjects.Add(sub1);

            // 2. English for Communication (A >= 84.50)
            GradeScheme engScheme = new GradeScheme
            {
                A = 84.50,
                BPlus = 79.50,
                B = 74.50,
                CPlus = 69.50,
                C = 64.50,
                DPlus = 59.50,
                D = 54.50
            };
            Subject sub2 = new Subject("English for Communication", engScheme);
            sub2.Criteria.Add(new ScoreItem("Narrative paragraph self-study VDO worksheet", 5, 5, true));
            sub2.Criteria.Add(new ScoreItem("Narrative paragraph writing quiz", 15, 15, true));
            sub2.Criteria.Add(new ScoreItem("Interview", 10, 10, true));
            sub2.Criteria.Add(new ScoreItem("Quiz", 80, 30, true));
            sub2.Criteria.Add(new ScoreItem("Class participation", 5, 5, true));
            sub2.Criteria.Add(new ScoreItem("Product idea worksheet", 5, 5, true));
            sub2.Criteria.Add(new ScoreItem("Group discussion", 18, 15, true));
            sub2.Criteria.Add(new ScoreItem("Creative product group presentation", 21, 15, true));
            subjects.Add(sub2);

            // 3. Chinese (A >= 80)
            Subject sub3 = new Subject("Chinese", new GradeScheme());
            sub3.Criteria.Add(new ScoreItem("Writing Chinese Character (10 times)", 10, 10, true));
            sub3.Criteria.Add(new ScoreItem("Exercise (10 times)", 10, 10, true));
            sub3.Criteria.Add(new ScoreItem("MDL Test (11 times)", 5, 5, true));
            sub3.Criteria.Add(new ScoreItem("Oral Test (2 times)", 20, 10, true));
            sub3.Criteria.Add(new ScoreItem("Class activity (10 times)", 10, 5, true));
            sub3.Criteria.Add(new ScoreItem("Final Project", 10, 10, true));
            sub3.Criteria.Add(new ScoreItem("Midterm examination", 50, 25, true));
            sub3.Criteria.Add(new ScoreItem("Final examination", 45, 25, true));
            subjects.Add(sub3);

            // 4. Psychological in Daily Life (A >= 80)
            Subject sub4 = new Subject("Psychological in Daily Life", new GradeScheme());
            sub4.Criteria.Add(new ScoreItem("Quizzes", 20, 20, true));
            sub4.Criteria.Add(new ScoreItem("Mid-term examination", 80, 30, true));
            sub4.Criteria.Add(new ScoreItem("Assignment", 15, 15, true));
            sub4.Criteria.Add(new ScoreItem("Final examination", 80, 30, true));
            sub4.Criteria.Add(new ScoreItem("Attendance", 5, 5, true));
            subjects.Add(sub4);

            // 5. Global Citizenship (A >= 80)
            Subject sub5 = new Subject("Global Citizenship", new GradeScheme());
            sub5.Criteria.Add(new ScoreItem("SDGs VDO", 10, 10, true));
            sub5.Criteria.Add(new ScoreItem("VDO quizzes", 50, 50, true));
            sub5.Criteria.Add(new ScoreItem("Group Project", 40, 40, true));
            subjects.Add(sub5);
        }

        // ====================================================
        // 1. ดูและจัดการตารางเรียน
        // ====================================================
        static void ManageWeeklySchedule()
        {
            Console.Clear();
            Console.WriteLine("=========================================================================================");
            Console.WriteLine("                               WEEKLY TIMETABLE (ตารางเรียนประจำสัปดาห์)                   ");
            Console.WriteLine("=========================================================================================");

            bool hasAnyClass = false;

            for (int day = 0; day < 7; day++)
            {
                if (weeklySchedule[day].Count > 0)
                {
                    hasAnyClass = true;
                    Console.WriteLine($"\n📌 {dayNames[day]}");
                    Console.WriteLine("-----------------------------------------------------------------------------------------");

                    var sortedSlots = weeklySchedule[day].OrderBy(x => x.StartMinutes).ToList();
                    foreach (var s in sortedSlots)
                    {
                        string timeRange = s.GetFormattedTimeRange();
                        string subjectDisplay = string.IsNullOrEmpty(s.SectionInfo) || s.SectionInfo == "-"
                            ? s.SubjectCode
                            : $"{s.SubjectCode} ({s.SectionInfo})";

                        Console.WriteLine($"  เวลา : {timeRange,-14} วิชา : {subjectDisplay,-32} ห้อง : {s.RoomInfo,-15}");
                    }
                }
            }

            if (!hasAnyClass)
            {
                Console.WriteLine("\n                                  ยังไม่มีวิชาเรียนในตาราง                                 ");
            }

            Console.WriteLine("\n=========================================================================================");

            if (GetYesNoInput("\nต้องการเพิ่ม/แก้ไขตารางเรียนหรือไม่? (พิมพ์ yes หรือ no): "))
            {
                int? selectedDay = GetValidIntInput("เลือกวัน (0:จันทร์, 1:อังคาร, 2:พุธ, 3:พฤหัส, 4:ศุกร์, 5:เสาร์, 6:อาทิตย์) [Enter กลับเมนูหลัก]: ", 0, 6);
                if (selectedDay == null) return;

                Console.Write("กรอกรหัส/ชื่อวิชา (เช่น Computer Programming) [Enter กลับเมนูหลัก]: ");
                string subCode = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(subCode)) return;

                Console.Write("กรอกกลุ่มเรียน/ประเภท (เช่น LECT-1 หรือ - หากไม่มี): ");
                string secInfo = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(secInfo)) secInfo = "-";

                Console.Write("กรอกห้องเรียน (เช่น E3 B 101): ");
                string roomInfo = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(roomInfo)) roomInfo = "-";

                int? startMins = GetFlexibleTimeInput("เวลาเริ่มต้น (ใส่นาทีได้อิสระ เช่น 10.15, 13.45) [Enter กลับเมนูหลัก]: ");
                if (startMins == null) return;

                int? endMins = GetFlexibleTimeInput("เวลาสิ้นสุด (ใส่นาทีได้อิสระ เช่น 12.10, 15.30) [Enter กลับเมนูหลัก]: ");
                if (endMins == null) return;

                while (endMins.Value <= startMins.Value)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("⚠️ ผู้ใช้กรอกเวลาไม่ถูกต้อง! เวลาสิ้นสุดต้องมากกว่าเวลาเริ่มต้น");
                    Console.ResetColor();
                    endMins = GetFlexibleTimeInput("กรอกเวลาสิ้นสุดใหม่อีกครั้ง [หรือกด Enter กลับหน้าเมนูหลัก]: ");
                    if (endMins == null) return;
                }

                var overlappingSlots = weeklySchedule[selectedDay.Value]
                    .Where(s => startMins.Value < s.EndMinutes && endMins.Value > s.StartMinutes)
                    .ToList();

                if (overlappingSlots.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️ พบวิชาเรียนที่มีช่วงเวลาซ้อนทับกันในวันดังกล่าว!");
                    Console.ResetColor();
                    if (!GetYesNoInput("คุณต้องการลบวิชาที่ซ้อนทับและบันทึกวิชาใหม่แทนหรือไม่? (พิมพ์ yes หรือ no): "))
                    {
                        Console.WriteLine("\n[ยกเลิกการเพิ่มวิชา] กด Enter เพื่อกลับหน้าเมนูหลัก...");
                        Console.ReadLine();
                        return;
                    }

                    foreach (var oldSlot in overlappingSlots)
                    {
                        weeklySchedule[selectedDay.Value].Remove(oldSlot);
                    }
                }

                weeklySchedule[selectedDay.Value].Add(new ScheduleSlot(subCode, secInfo, roomInfo, startMins.Value, endMins.Value));
                Console.WriteLine("\n✅ [อัปเดตวิชาลงในตารางเรียนชั่วคราวแล้ว! (อย่าลืมกดเมนู 8 เพื่อบันทึกลงไฟล์)]");
            }

            Console.WriteLine("\nกด Enter เพื่อกลับหน้าเมนูหลัก...");
            Console.ReadLine();
        }

        // ====================================================
        // 2. บันทึกคะแนนเก็บและคำนวณเกรดรายวิชา
        // ====================================================
        static void InputScoresAndCalculateGrade()
        {
            Console.Clear();
            Console.WriteLine("=== บันทึกคะแนนและคำนวณเกรดรายวิชา ===");

            for (int i = 0; i < subjects.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {subjects[i].Name}");
            }

            int? choice = GetValidIntInput($"\nเลือกวิชาที่ต้องการบันทึกคะแนน (1-{subjects.Count}) [กด Enter กลับหน้าเมนูหลัก]: ", 1, subjects.Count);
            if (choice == null) return;

            Subject sub = subjects[choice.Value - 1];
            Console.WriteLine($"\n--- บันทึกคะแนนวิชา: {sub.Name} ---");

            bool hasUnentered = sub.Criteria.Any(c => !c.IsEntered);
            bool onlyFillMissing = false;

            if (hasUnentered && sub.Criteria.Any(c => c.IsEntered))
            {
                Console.WriteLine("\nพบรายการคะแนนที่ยังไม่ได้กรอกบางรายการ ต้องการกรอกแบบใด?:");
                Console.WriteLine(" 1. กรอกเฉพาะรายการที่เว้นไว้ (ข้ามรายการที่เคยกรอกแล้ว)");
                Console.WriteLine(" 2. กรอกคะแนนใหม่ทั้งหมด");
                int? fillChoice = GetValidIntInput("เลือก (1 หรือ 2) [กด Enter กลับหน้าเมนูหลัก]: ", 1, 2);
                if (fillChoice == null) return;
                if (fillChoice.Value == 1) onlyFillMissing = true;
            }

            Console.WriteLine("\n(หากรายการไหนยังไม่ทราบคะแนน ให้พิมพ์ '-' เพื่อเว้นไว้ หรือกด Enter เปล่าๆ เพื่อกลับหน้าเมนูหลัก)");

            for (int i = 0; i < sub.Criteria.Count; i++)
            {
                ScoreItem item = sub.Criteria[i];

                if (onlyFillMissing && item.IsEntered)
                {
                    double earnedPct = item.IsPercentage ? (item.EarnedScore / item.MaxScore) * item.WeightPercentage : item.EarnedScore;
                    Console.WriteLine($"คะแนน '{item.ItemName}': {item.EarnedScore:F2}/{item.MaxScore} (คิดเป็น {earnedPct:F2}%) - ข้าม");
                    continue;
                }

                string infoLabel = item.IsPercentage
                    ? $"เต็ม {item.MaxScore} คะแนน -> น้ำหนัก {item.WeightPercentage}%"
                    : $"คะแนนเต็ม {item.MaxScore}";

                string currentStatus = item.IsEntered ? $"{item.EarnedScore:F2}/{item.MaxScore}" : "ยังไม่กรอก";

                while (true)
                {
                    Console.Write($"คะแนน '{item.ItemName}' [{infoLabel}] (ปัจจุบัน: {currentStatus}) -> กรอกคะแนนที่ได้: ");
                    string scoreInput = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(scoreInput))
                    {
                        Console.WriteLine("\n[กลับหน้าเมนูหลัก]");
                        return;
                    }

                    if (scoreInput == "-")
                    {
                        Console.WriteLine("   [เว้นรายการนี้ไว้]");
                        break;
                    }

                    if (double.TryParse(scoreInput, out double scoreVal) && scoreVal >= 0 && scoreVal <= item.MaxScore)
                    {
                        item.EarnedScore = scoreVal;
                        item.IsEntered = true;
                        if (item.IsPercentage)
                        {
                            double calcPct = (scoreVal / item.MaxScore) * item.WeightPercentage;
                            Console.WriteLine($"   -> บันทึกสำเร็จ! ({scoreVal}/{item.MaxScore} คะแนน = {calcPct:F2}%)");
                        }
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"⚠️ ผู้ใช้กรอกคะแนนไม่ถูกต้อง! ต้องเป็นตัวเลขระหว่าง 0 ถึง {item.MaxScore}");
                        Console.WriteLine("   -> กรุณาพิมพ์ใหม่, พิมพ์ '-' เพื่อข้าม, หรือ [กด Enter เปล่าๆ เพื่อกลับหน้าเมนูหลัก]");
                        Console.ResetColor();
                    }
                }
            }

            double total = sub.CalculateTotalScore100();
            string grade = sub.Scheme.GetGrade(total);

            Console.WriteLine($"\n--> คะแนนรวมสะสมปัจจุบัน: {total:F2} / 100");
            Console.WriteLine($"--> เกรดคาดการณ์ปัจจุบัน: {grade}");
            Console.WriteLine("(อย่าลืมกดเมนู 8 เพื่อบันทึกข้อมูลลงไฟล์)");

            Console.WriteLine("\nกด Enter เพื่อกลับหน้าเมนูหลัก...");
            Console.ReadLine();
        }

        // ====================================================
        // 3. เพิ่มรายชื่อวิชาใหม่
        // ====================================================
        static void AddNewCustomSubject()
        {
            Console.Clear();
            Console.WriteLine("=== เพิ่มรายชื่อวิชาใหม่และกำหนดเกณฑ์คะแนนเอง ===");
            Console.Write("กรอกชื่อวิชาใหม่ [กด Enter เปล่าๆ เพื่อกลับหน้าเมนูหลัก]: ");
            string subName = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(subName)) return;

            GradeScheme scheme = new GradeScheme();

            if (!GetYesNoInput("ใช้อัตราตัดเกรดแบบ Computer Programming (A >= 80.00) หรือไม่? (พิมพ์ yes หรือ no): "))
            {
                Console.WriteLine("\n--- เลือกปรับแต่งเกณฑ์การตัดเกรด ---");
                Console.WriteLine("1. ปรับเกรด A   2. ปรับเกรด B+  3. ปรับเกรด B   4. ปรับเกรด C+");
                Console.WriteLine("5. ปรับเกรด C   6. ปรับเกรด D+  7. ปรับเกรด D   8. ปรับเปลี่ยนทั้งหมด");
                int? gChoice = GetValidIntInput("เลือกรายการ (1-8) [กด Enter กลับหน้าเมนูหลัก]: ", 1, 8);
                if (gChoice == null) return;

                double? val;
                switch (gChoice.Value)
                {
                    case 1: val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด A: ", 0, 100); if (val == null) return; scheme.A = val.Value; break;
                    case 2: val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด B+: ", 0, 100); if (val == null) return; scheme.BPlus = val.Value; break;
                    case 3: val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด B: ", 0, 100); if (val == null) return; scheme.B = val.Value; break;
                    case 4: val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด C+: ", 0, 100); if (val == null) return; scheme.CPlus = val.Value; break;
                    case 5: val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด C: ", 0, 100); if (val == null) return; scheme.C = val.Value; break;
                    case 6: val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด D+: ", 0, 100); if (val == null) return; scheme.DPlus = val.Value; break;
                    case 7: val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด D: ", 0, 100); if (val == null) return; scheme.D = val.Value; break;
                    case 8:
                        val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด A: ", 0, 100); if (val == null) return; scheme.A = val.Value;
                        val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด B+: ", 0, 100); if (val == null) return; scheme.BPlus = val.Value;
                        val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด B: ", 0, 100); if (val == null) return; scheme.B = val.Value;
                        val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด C+: ", 0, 100); if (val == null) return; scheme.CPlus = val.Value;
                        val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด C: ", 0, 100); if (val == null) return; scheme.C = val.Value;
                        val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด D+: ", 0, 100); if (val == null) return; scheme.DPlus = val.Value;
                        val = GetValidDoubleInput("กรอกคะแนนขั้นต่ำเกรด D: ", 0, 100); if (val == null) return; scheme.D = val.Value;
                        break;
                }
            }

            Subject newSub = new Subject(subName, scheme);

            int? count = GetValidIntInput("\nต้องการแบ่งรายการเกณฑ์เก็บคะแนนกี่รายการ? (1-20) [กด Enter กลับหน้าเมนูหลัก]: ", 1, 20);
            if (count == null) return;

            for (int i = 0; i < count.Value; i++)
            {
                Console.WriteLine($"\nรายการที่ {i + 1}:");
                Console.Write(" - ชื่อชิ้นงาน/การสอบ [กด Enter กลับหน้าเมนูหลัก]: ");
                string itemName = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(itemName)) return;

                int? typeChoice = GetValidIntInput(" - รูปแบบการเก็บคะแนน (1: สัดส่วนเปอร์เซ็นต์ %, 2: คะแนนเต็มจริง) [Enter กลับเมนูหลัก]: ", 1, 2);
                if (typeChoice == null) return;

                bool isPercent = (typeChoice.Value == 1);

                if (isPercent)
                {
                    double? maxRawScore = GetValidDoubleInput(" - คะแนนเต็มของงานนี้ที่อาจารย์กำหนด (เช่น 50 หรือ 100) [Enter กลับเมนูหลัก]: ", 0.1, 10000);
                    if (maxRawScore == null) return;

                    double? weightPct = GetValidDoubleInput(" - สัดส่วนเปอร์เซ็นต์ (%) ของงานนี้ (เช่น 20) [Enter กลับเมนูหลัก]: ", 0.1, 100);
                    if (weightPct == null) return;

                    newSub.Criteria.Add(new ScoreItem(itemName, maxRawScore.Value, weightPct.Value, true));
                }
                else
                {
                    double? maxRawScore = GetValidDoubleInput(" - คะแนนเต็มของงานนี้: ", 0.1, 10000);
                    if (maxRawScore == null) return;

                    newSub.Criteria.Add(new ScoreItem(itemName, maxRawScore.Value, 0, false));
                }
            }

            subjects.Add(newSub);
            Console.WriteLine($"\n✅ [เพิ่มวิชา {subName} เข้าสู่ระบบชั่วคราวแล้ว! (อย่าลืมกดเมนู 8 เพื่อบันทึกลงไฟล์)]");

            Console.WriteLine("\nกด Enter เพื่อกลับหน้าเมนูหลัก...");
            Console.ReadLine();
        }

        // ====================================================
        // 4. คำนวณคะแนนที่ขาดเพื่ออัปเกรด
        // ====================================================
        static void ShowPointsNeededForNextGrade()
        {
            Console.Clear();
            Console.WriteLine("=======================================================================");
            Console.WriteLine("            ระบบคำนวณคะแนนที่ขาดเพื่ออัปเกรด (แจกแจงทุกระดับเกรด)         ");
            Console.WriteLine("=======================================================================");

            foreach (var sub in subjects)
            {
                double currentScore = sub.CalculateTotalScore100();
                string currentGrade = sub.Scheme.GetGrade(currentScore);

                Console.WriteLine($"\n📘 วิชา: {sub.Name}");
                Console.WriteLine($"   คะแนนสะสมปัจจุบัน: {currentScore:F2} / 100 | เกรดปัจจุบัน: {currentGrade}");
                Console.WriteLine("   -------------------------------------------------------------------");

                PrintNeededScore(currentScore, sub.Scheme.A, "เกรด A");
                PrintNeededScore(currentScore, sub.Scheme.BPlus, "เกรด B+");
                PrintNeededScore(currentScore, sub.Scheme.B, "เกรด B");
                PrintNeededScore(currentScore, sub.Scheme.CPlus, "เกรด C+");
                PrintNeededScore(currentScore, sub.Scheme.C, "เกรด C");
                PrintNeededScore(currentScore, sub.Scheme.DPlus, "เกรด D+");
                PrintNeededScore(currentScore, sub.Scheme.D, "เกรด D");
            }

            Console.WriteLine("\nกด Enter เพื่อกลับหน้าเมนูหลัก...");
            Console.ReadLine();
        }

        static void PrintNeededScore(double currentScore, double targetThreshold, string gradeName)
        {
            if (currentScore >= targetThreshold)
            {
                Console.WriteLine($"   ✅ {gradeName,-7}: ได้ระดับเกรดนี้แล้ว (เกณฑ์ {targetThreshold:F2} คะแนน)");
            }
            else
            {
                double needed = targetThreshold - currentScore;
                Console.WriteLine($"   🎯 {gradeName,-7}: ขาดอีก {needed,6:F2} คะแนน (ต้องการคะแนนรวม {targetThreshold:F2})");
            }
        }

        // ====================================================
        // 5. จัดลำดับความสำคัญวิชาที่ควรโฟกัสอ่านหนังสือก่อน
        // ====================================================
        static void ShowStudyPriority()
        {
            Console.Clear();
            Console.WriteLine("=== จัดลำดับความสำคัญวิชาที่ควรโฟกัสอ่านหนังสือก่อน ===");
            Console.WriteLine("(วิชาที่ได้คะแนนสะสมน้อยที่สุด จะถูกจัดไว้เป็นอันดับที่ 1)\n");

            var sortedSubjects = subjects.OrderBy(s => s.CalculateTotalScore100()).ToList();

            for (int i = 0; i < sortedSubjects.Count; i++)
            {
                var sub = sortedSubjects[i];
                double score = sub.CalculateTotalScore100();
                string grade = sub.Scheme.GetGrade(score);

                Console.WriteLine($"อันดับ {i + 1} 📌 วิชา: {sub.Name}");
                Console.WriteLine($"   คะแนนสะสม: {score:F2} / 100 | เกรดปัจจุบัน: {grade}");

                if (score < sub.Scheme.D)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("   ⚠️ สถานะ: วิกฤต! เสี่ยงติด F ต้องเร่งอ่านหนังสือ/ทบทวนด่วน");
                    Console.ResetColor();
                }
                else if (score < sub.Scheme.C)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("   ⚡ สถานะ: โซนคะแนนน้อย ควรจัดเวลาอ่านหนังสือเพิ่มเป็นอันดับแรกๆ");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("   ✅ สถานะ: คะแนนอยู่ในเกณฑ์ปลอดภัย");
                    Console.ResetColor();
                }
                Console.WriteLine("-----------------------------------------------------------------------");
            }

            Console.WriteLine("\nกด Enter เพื่อกลับหน้าเมนูหลัก...");
            Console.ReadLine();
        }

        // ====================================================
        // 6. ดูสรุปข้อมูลรวมทั้งหมดของแต่ละวิชา (เลือกดูรายวิชาได้)
        // ====================================================
        static void ShowComprehensiveSummary()
        {
            Console.Clear();
            Console.WriteLine("=======================================================================");
            Console.WriteLine("                    รายงานข้อมูลภาพรวมรายวิชาทั้งหมด                     ");
            Console.WriteLine("=======================================================================");

            for (int i = 0; i < subjects.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {subjects[i].Name}");
            }

            int? choice = GetValidIntInput($"\nเลือกวิชาที่ต้องการดูสรุปข้อมูล (1-{subjects.Count}) [กด Enter กลับหน้าเมนูหลัก]: ", 1, subjects.Count);
            if (choice == null) return;

            var sortedSubjects = subjects.OrderBy(s => s.CalculateTotalScore100()).ToList();
            Subject sub = subjects[choice.Value - 1];

            double score = sub.CalculateTotalScore100();
            string grade = sub.Scheme.GetGrade(score);
            int priorityRank = sortedSubjects.IndexOf(sub) + 1;

            Console.WriteLine("\n-----------------------------------------------------------------------");
            Console.WriteLine($"📘 วิชา: {sub.Name}");
            Console.WriteLine($" 1. คะแนนสะสมรวมปัจจุบัน: {score:F2} / 100 คะแนน");
            Console.WriteLine($" 2. เกรดปัจจุบัน: {grade}");

            if (grade == "A")
            {
                Console.WriteLine(" 3. เป้าหมายเกรดถัดไป: บรรลุเกรดสูงสุดแล้ว (A)");
            }
            else
            {
                double nextTarget = sub.Scheme.GetNextGradeTarget(score, out string nextGrade);
                Console.WriteLine($" 3. เป้าหมายเกรดถัดไป: ขาดอีก {nextTarget - score:F2} คะแนน เพื่อให้ได้เกรด {nextGrade}");
            }

            Console.WriteLine($" 4. ลำดับความสำคัญอ่านหนังสือ: อันดับที่ {priorityRank} จาก {subjects.Count} วิชา");

            Console.WriteLine(" 5. รายการคะแนนเก็บ:");
            foreach (var item in sub.Criteria)
            {
                if (item.IsPercentage)
                {
                    double pctEarned = item.IsEntered ? (item.EarnedScore / item.MaxScore) * item.WeightPercentage : 0;
                    string status = item.IsEntered ? $"{item.EarnedScore:F2}/{item.MaxScore} (ได้ {pctEarned:F2}% จาก {item.WeightPercentage}%)" : $"- / {item.MaxScore} (น้ำหนัก {item.WeightPercentage}%)";
                    Console.WriteLine($"    - {item.ItemName}: {status}");
                }
                else
                {
                    string status = item.IsEntered ? $"{item.EarnedScore:F2} / {item.MaxScore} คะแนน" : $"- / {item.MaxScore} คะแนน";
                    Console.WriteLine($"    - {item.ItemName}: {status}");
                }
            }
            Console.WriteLine("-----------------------------------------------------------------------");

            Console.WriteLine("\nกด Enter เพื่อกลับหน้าเมนูหลัก...");
            Console.ReadLine();
        }

        // ====================================================
        // 7. ล้างข้อมูล (Clear Data)
        // ====================================================
        static void ResetDataMenu()
        {
            Console.Clear();
            Console.WriteLine("=== เมนูล้างข้อมูล (Clear Data) ===");
            Console.WriteLine(" 1. ล้างคะแนนเก็บทั้งหมด (ทุกวิชา)");
            Console.WriteLine(" 2. ล้างคะแนนเก็บเฉพาะวิชาที่เลือก");
            Console.WriteLine(" 3. ล้างตารางเรียนทั้งหมด");
            Console.WriteLine(" 4. ล้างตารางเรียนเฉพาะวันและวิชาที่ต้องการ");
            Console.WriteLine(" 5. ล้างข้อมูลทั้งหมดและรีเซ็ตเป็นค่าเริ่มต้น (5 วิชาเริ่มต้น + ตารางว่าง)");
            Console.WriteLine(" 6. ยกเลิกและกลับเมนูหลัก");

            int? choice = GetValidIntInput("เลือกรายการ (1-6) [กด Enter กลับหน้าเมนูหลัก]: ", 1, 6);
            if (choice == null || choice.Value == 6) return;

            switch (choice.Value)
            {
                case 1:
                    if (GetYesNoInput("\n⚠️ ยืนยันล้างคะแนนเก็บทุกวิชาหรือไม่? (พิมพ์ yes หรือ no): "))
                    {
                        foreach (var sub in subjects)
                        {
                            foreach (var item in sub.Criteria) { item.EarnedScore = 0; item.IsEntered = false; }
                        }
                        Console.WriteLine("\n✅ [ล้างข้อมูลคะแนนทุกวิชาเรียบร้อยแล้ว (อย่าลืมกดเมนู 8 เพื่อบันทึกลงไฟล์)]");
                    }
                    break;

                case 2:
                    Console.WriteLine("\n--- เลือกวิชาที่ต้องการล้างคะแนน ---");
                    for (int i = 0; i < subjects.Count; i++) Console.WriteLine($"{i + 1}. {subjects[i].Name}");
                    int? subIdx = GetValidIntInput($"เลือกวิชา (1-{subjects.Count}) [กด Enter กลับหน้าเมนูหลัก]: ", 1, subjects.Count);
                    if (subIdx == null) return;

                    Subject targetSub = subjects[subIdx.Value - 1];
                    if (GetYesNoInput($"⚠️ ยืนยันล้างคะแนนของวิชา '{targetSub.Name}' หรือไม่? (พิมพ์ yes หรือ no): "))
                    {
                        foreach (var item in targetSub.Criteria) { item.EarnedScore = 0; item.IsEntered = false; }
                        Console.WriteLine($"\n✅ [ล้างคะแนนวิชา {targetSub.Name} เรียบร้อยแล้ว (อย่าลืมกดเมนู 8 เพื่อบันทึกลงไฟล์)]");
                    }
                    break;

                case 3:
                    if (GetYesNoInput("\n⚠️ ยืนยันล้างตารางเรียนทั้งหมดหรือไม่? (พิมพ์ yes หรือ no): "))
                    {
                        for (int i = 0; i < 7; i++) weeklySchedule[i].Clear();
                        Console.WriteLine("\n✅ [ล้างตารางเรียนทั้งหมดเรียบร้อยแล้ว (อย่าลืมกดเมนู 8 เพื่อบันทึกลงไฟล์)]");
                    }
                    break;

                case 4:
                    int? day = GetValidIntInput("\nเลือกวันที่จะล้างตาราง (0:จันทร์, 1:อังคาร, 2:พุธ, 3:พฤหัส, 4:ศุกร์, 5:เสาร์, 6:อาทิตย์) [Enter กลับเมนูหลัก]: ", 0, 6);
                    if (day == null) return;

                    if (weeklySchedule[day.Value].Count == 0)
                    {
                        Console.WriteLine($"\n[{dayNames[day.Value]} ไม่มีวิชาเรียนให้ลบ]");
                    }
                    else
                    {
                        var sorted = weeklySchedule[day.Value].OrderBy(x => x.StartMinutes).ToList();
                        Console.WriteLine($"\nรายการวิชาเรียนใน{dayNames[day.Value]}:");
                        for (int i = 0; i < sorted.Count; i++)
                        {
                            Console.WriteLine($" {i + 1}. {sorted[i].GetFormattedTimeRange()} | {sorted[i].SubjectCode} ({sorted[i].RoomInfo})");
                        }

                        int? delIdx = GetValidIntInput($"เลือกลำดับวิชาที่ต้องการลบ (1-{sorted.Count}) [Enter กลับเมนูหลัก]: ", 1, sorted.Count);
                        if (delIdx == null) return;

                        ScheduleSlot slotToRemove = sorted[delIdx.Value - 1];
                        if (GetYesNoInput($"⚠️ ยืนยันลบวิชา '{slotToRemove.SubjectCode}' เวลา {slotToRemove.GetFormattedTimeRange()} หรือไม่? (พิมพ์ yes หรือ no): "))
                        {
                            weeklySchedule[day.Value].Remove(slotToRemove);
                            Console.WriteLine("\n✅ [ลบรายวิชาออกจากตารางเรียนเรียบร้อยแล้ว (อย่าลืมกดเมนู 8 เพื่อบันทึกลงไฟล์)]");
                        }
                    }
                    break;

                case 5:
                    if (GetYesNoInput("\n⚠️ ยืนยันล้างข้อมูลทั้งหมด (รวมวิชาที่เพิ่มเอง คะแนน และตารางเรียน) หรือไม่? (พิมพ์ yes หรือ no): "))
                    {
                        for (int i = 0; i < 7; i++) weeklySchedule[i].Clear();
                        InitializeDefaultSubjects();
                        Console.WriteLine("\n✅ [รีเซ็ตข้อมูลทั้งหมดเรียบร้อยแล้ว (อย่าลืมกดเมนู 8 เพื่อบันทึกลงไฟล์)]");
                    }
                    break;
            }

            Console.WriteLine("\nกด Enter เพื่อกลับหน้าเมนูหลัก...");
            Console.ReadLine();
        }

        // ====================================================
        // 8. Save & Load Data
        // ====================================================
        static void SaveDataToFile()
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                foreach (var sub in subjects)
                {
                    writer.WriteLine($"SUBJECT|{sub.Name}|{sub.Scheme.A}|{sub.Scheme.BPlus}|{sub.Scheme.B}|{sub.Scheme.CPlus}|{sub.Scheme.C}|{sub.Scheme.DPlus}|{sub.Scheme.D}");
                    foreach (var item in sub.Criteria)
                    {
                        writer.WriteLine($"CRITERIA|{item.ItemName}|{item.MaxScore}|{item.WeightPercentage}|{item.EarnedScore}|{item.IsPercentage}|{item.IsEntered}");
                    }
                }

                for (int i = 0; i < 7; i++)
                {
                    foreach (var s in weeklySchedule[i])
                    {
                        writer.WriteLine($"SCHEDULE_V2|{i}|{s.SubjectCode}|{s.SectionInfo}|{s.RoomInfo}|{s.StartMinutes}|{s.EndMinutes}");
                    }
                }
                writer.Flush();
            }
        }

        static void LoadDataFromFile()
        {
            if (!File.Exists(filePath)) return;

            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
            if (lines.Length == 0) return;

            subjects.Clear();
            for (int i = 0; i < 7; i++) weeklySchedule[i].Clear();

            Subject currentSubject = null;

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split('|');
                if (parts[0] == "SUBJECT")
                {
                    GradeScheme scheme = new GradeScheme
                    {
                        A = double.Parse(parts[2]),
                        BPlus = double.Parse(parts[3]),
                        B = double.Parse(parts[4]),
                        CPlus = double.Parse(parts[5]),
                        C = double.Parse(parts[6]),
                        DPlus = double.Parse(parts[7]),
                        D = double.Parse(parts[8])
                    };
                    currentSubject = new Subject(parts[1], scheme);
                    subjects.Add(currentSubject);
                }
                else if (parts[0] == "CRITERIA" && currentSubject != null)
                {
                    if (parts.Length >= 7)
                    {
                        ScoreItem item = new ScoreItem(parts[1], double.Parse(parts[2]), double.Parse(parts[3]), bool.Parse(parts[5]))
                        {
                            EarnedScore = double.Parse(parts[4]),
                            IsEntered = bool.Parse(parts[6])
                        };
                        currentSubject.Criteria.Add(item);
                    }
                    else
                    {
                        double max = double.Parse(parts[2]);
                        ScoreItem item = new ScoreItem(parts[1], max, max, bool.Parse(parts[4]))
                        {
                            EarnedScore = double.Parse(parts[3]),
                            IsEntered = bool.Parse(parts[5])
                        };
                        currentSubject.Criteria.Add(item);
                    }
                }
                else if (parts[0] == "SCHEDULE_V2")
                {
                    int day = int.Parse(parts[1]);
                    int startMins = int.Parse(parts[5]);
                    int endMins = int.Parse(parts[6]);
                    weeklySchedule[day].Add(new ScheduleSlot(parts[2], parts[3], parts[4], startMins, endMins));
                }
            }
        }
    }
}
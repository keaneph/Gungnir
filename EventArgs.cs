// EventArgs.cs
using System;
using sis_app;

namespace glaive
{
    public class CollegeEventArgs : EventArgs
    {
        public College College { get; }
        public CollegeEventArgs(College college)
        {
            College = college;
        }
    }

    public class ProgramEventArgs : EventArgs
    {
        public Program Program { get; }
        public ProgramEventArgs(Program program)
        {
            Program = program;
        }
    }

    public class StudentEventArgs : EventArgs
    {
        public Student Student { get; }
        public StudentEventArgs(Student student)
        {
            Student = student;
        }
    }

    public class HistoryEntryEventArgs : EventArgs
    {
        public HistoryEntry Entry { get; }
        public HistoryEntryEventArgs(HistoryEntry entry)
        {
            Entry = entry;
        }
    }
}
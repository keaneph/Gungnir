// ProgramEventArgs.cs
public class ProgramEventArgs : EventArgs
{
    public Program Program { get; }
    public ProgramEventArgs(Program program) => Program = program;
}
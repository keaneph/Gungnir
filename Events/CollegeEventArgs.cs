// CollegeEventArgs.cs
public class CollegeEventArgs : EventArgs
{
    public College College { get; }
    public CollegeEventArgs(College college) => College = college;
}
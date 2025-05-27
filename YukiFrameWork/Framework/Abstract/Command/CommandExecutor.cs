///=====================================================
/// - FileName:      CommandExecutor.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/5/25 10:51:14
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
namespace YukiFrameWork
{
    /// <summary>
    /// 执行命令具体处理的接口。实现该接口可自定义对于命令的执行与撤销实现。
    /// <para>命令的发送撤销均通过该接口具体执行。框架会有默认的调用方式。如有需求可自行定义如下</para>
    /// <code>public class CustomCommandExecutor : ICommandExecutor
    /// {
    ///     //实现接口所需要的方法
    /// }
    /// </code>
    /// 在架构中设置Executor
    /// <code>CommandExecutor = new CustomCommandExecutor()</code>
    /// </summary>
    public interface ICommandExecutor
    {
        /// <summary>
        /// 命令执行中枢在每一次执行命令时会赋值架构，无需用户考虑
        /// </summary>
        IArchitecture Architecture { get; set; }
        void Execute(ICommand command);
        TResult Execute<TResult>(ICommand<TResult> command);

        void Undo(IUndoCommand command);
        TResult Undo<TResult>(IUndoCommand<TResult> command);
    }

    internal class DefaultCommandExecutor : ICommandExecutor
    {
        public IArchitecture Architecture { get ; set; }

        public void Execute(ICommand command)
        {
            command.SetArchitecture(Architecture);
            command.Execute();
        }

        public TResult Execute<TResult>(ICommand<TResult> command)
        {
            command.SetArchitecture(Architecture);
            return command.Execute();
        }

        public void Undo(IUndoCommand undo)
        {
            undo.SetArchitecture(Architecture);
            undo.Undo();

        }

        public TResult Undo<TResult>(IUndoCommand<TResult> undo)
        {
            undo.SetArchitecture(Architecture);
            return undo.Undo();
        }
    }
}

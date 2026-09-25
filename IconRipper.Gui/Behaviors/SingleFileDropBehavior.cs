using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Xaml.Interactions.DragAndDrop;

namespace IconRipper.Gui.Behaviors;

public sealed class SingleFileDropBehavior : DropBehaviorBase
{
    public bool AlwaysAddClass { get; set; } = true;

    public SingleFileDropBehavior()
    {
        PassEventArgsToCommand = true;
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        Handler = new SingleFileDropHandler(ExecuteCommand, AlwaysAddClass);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject?.Classes.Remove("dragging-over");
        Handler = null;
    }

    private sealed class SingleFileDropHandler(Action<object?> execute, bool alwaysAddClass) : DropHandlerBase
    {
        private static bool TryGetSingleFile(DragEventArgs e, [NotNullWhen(true)] out IStorageFile? storageFile)
        {
            storageFile = null;

            if (!e.DataTransfer.Contains(DataFormat.File))
                return false;

            if (e.DataTransfer.TryGetFiles() is not { Length: 1 } files)
                return false;

            return (storageFile = files[0] as IStorageFile) is not null;
        }

        public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext,
            object? state)
        {
            return TryGetSingleFile(e, out _);
        }

        public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext,
            object? state)
        {
            if (!TryGetSingleFile(e, out var storageFile))
                return false;

            execute(storageFile);

            return true;
        }

        public override void Over(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
        {
            base.Over(sender, e, sourceContext, targetContext);

            if (alwaysAddClass || e.DragEffects != DragDropEffects.None)
                (sender as Control).Classes.Add("dragging-over");
            else
                (sender as Control).Classes.Remove("dragging-over");
        }

        public override void Leave(object? sender, RoutedEventArgs e)
        {
            base.Leave(sender, e);

            (sender as Control).Classes.Remove("dragging-over");
        }

        public override void Drop(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
        {
            base.Drop(sender, e, sourceContext, targetContext);

            (sender as Control).Classes.Remove("dragging-over");
        }
    }
}
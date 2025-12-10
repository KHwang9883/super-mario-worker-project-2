using Godot;
using System;

public partial class MessageDisplay : Label {
    [Signal]
    public delegate void PlaySoundMessageDisplayEventHandler();
    [Signal]
    public delegate void PlaySoundMessageUndisplayEventHandler();
    
    private string _currentMessage = "";
    private MessageBlock? _currentMessageBlock;
    private bool _messageShown;

    private bool _show;
    private int _messageLength;
    private int _wordPointer;
    private int _characterAdvanceTimer;

    public override void _Ready() {
        // For debug usage
        _currentMessage = StringProcess.ConvertHashAndNewline(_currentMessage);
    }
    public override void _PhysicsProcess(double delta) {
        // Press Enter to undisplay
        if (Input.IsActionJustPressed("confirm")) {
            TextClear();
        }
        
        if (!_show) return;
        
        // 已经显示完毕过的则直接显示全部内容
        if (_messageShown) {
            Text = _currentMessage;
        }
        // 逐字显示
        else {
            if (_wordPointer >= _messageLength) {
                _currentMessageBlock?.SetShown();
                return;
            }
            
            // 判断字符是否是中文，如果是则显示时间增加一倍
            var characterAdvanceTime =
                (StringProcess.IsWideCharacter(_currentMessage[_wordPointer])) ? 2 : 1;
            _characterAdvanceTimer++;
            if (_characterAdvanceTimer < characterAdvanceTime) return;
            
            Text += _currentMessage[_wordPointer];
            _wordPointer++;
            _characterAdvanceTimer = 0;
        }
    }

    public void SetMessage(string message, bool shown, MessageBlock messageBlock) {
        // 如果是同一信息砖
        if (_currentMessageBlock == messageBlock) {
            // 同时在显示这个信息砖的文本，那么取消显示
            if (!Text.Equals("")) {
                TextClear();
                return;
            }
        }
       
        // 其他的信息砖
        _currentMessageBlock = messageBlock;
        _currentMessage = message;
        _messageShown = shown;
        
        _messageLength = message.Length;
        Text = "";
        _wordPointer = 0;
        _show = true;

        EmitSignal(SignalName.PlaySoundMessageDisplay);
    }
    public void TextClear() {
        // 清空不显示
        Text = "";
        _currentMessage = "";
        _wordPointer = 0;
        _show = false;
        EmitSignal(SignalName.PlaySoundMessageUndisplay);
    }
}

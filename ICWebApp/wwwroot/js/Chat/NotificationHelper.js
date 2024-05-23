function MarkStepAsUnread(ChatIsUnread) {
    var _allChatStepper = document.getElementsByClassName('wizard-chat-step');
    for (var _chatStepIndex = 0; _chatStepIndex < _allChatStepper.length; _chatStepIndex++) {
        var _allChatStepperClasses = _allChatStepper[_chatStepIndex].classList;
        if (!ChatIsUnread && _allChatStepperClasses.contains('unread-chat-message')) {
            // Remove class
            _allChatStepper[_chatStepIndex].classList.remove('unread-chat-message')
        }
        if (ChatIsUnread && !_allChatStepperClasses.contains('unread-chat-message')) {
            // Add class
            _allChatStepper[_chatStepIndex].classList.add('unread-chat-message')
        }
    }
}
document.getElementById('postQuestionForm').addEventListener('submit', function (event) {
    event.preventDefault();

    // Get form values
    var question = document.getElementById('question').value;
    var duration = document.getElementById('duration').value;

    // Perform AJAX to submit the question
    // Add your AJAX code here

    alert('Question Posted Successfully!');
});

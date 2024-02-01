document.getElementById('unsolicitedFeedbackForm').addEventListener('submit', function (event) {
    event.preventDefault();

    // Get form values
    var problemArea = document.getElementById('problemArea').value;
    var complaint = document.getElementById('complaint').value;
    var proposedSolution = document.getElementById('proposedSolution').value;

    // Perform AJAX to submit the feedback
    // Add your AJAX code here

    alert('Feedback Submitted Successfully!');
});

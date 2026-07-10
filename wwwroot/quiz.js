const API_BASE_URL = '/api';

let quizQuestions = [];
let currentQuestionIndex = 0;
let userAnswers = {};
let quizStartTime = null;

async function startQuiz() {
    try {
        const url = `${API_BASE_URL}/quiz/generate`;

        const response = await fetch(url, {
            method: 'POST'
        });
        quizQuestions = await response.json();

        if (quizQuestions.length === 0) {
            alert('Không có câu hỏi nào. Vui lòng thêm từ vựng trước!');
            return;
        }

        currentQuestionIndex = 0;
        userAnswers = {};
        quizStartTime = new Date();

        document.getElementById('quizSelection').style.display = 'none';
        document.getElementById('quizContainer').style.display = 'block';
        document.getElementById('totalQuestions').textContent = quizQuestions.length;

        displayQuestion();
    } catch (error) {
        console.error('Lỗi khi tải quiz:', error);
        alert('Lỗi khi tải bài quiz');
    }
}

function displayQuestion() {
    const question = quizQuestions[currentQuestionIndex];

    // Update progress
    const progress = ((currentQuestionIndex + 1) / quizQuestions.length) * 100;
    document.getElementById('progressFill').style.width = progress + '%';
    document.getElementById('currentQuestion').textContent = currentQuestionIndex + 1;

    // Display question
    const typeLabel = question.questionType === 'english_to_vietnamese' 
        ? 'Tiếng Anh → Tiếng Việt'
        : 'Tiếng Việt → Tiếng Anh';

    document.getElementById('questionType').textContent = typeLabel;
    document.getElementById('questionText').textContent = question.question;

    // Display options
    const optionsContainer = document.getElementById('optionsContainer');
    optionsContainer.innerHTML = '';

    question.options.forEach((option, index) => {
        const btn = document.createElement('button');
        btn.className = 'option-btn';
        btn.textContent = option;

        // Check if this option was previously selected
        if (userAnswers[currentQuestionIndex] === option) {
            btn.classList.add('selected');
        }

        btn.addEventListener('click', () => {
            // Remove previous selection
            document.querySelectorAll('.option-btn').forEach(b => b.classList.remove('selected'));
            // Add selection to current
            btn.classList.add('selected');
            // Save answer
            userAnswers[currentQuestionIndex] = option;
        });

        optionsContainer.appendChild(btn);
    });

    // Update navigation buttons
    document.getElementById('prevBtn').disabled = currentQuestionIndex === 0;
    document.getElementById('nextBtn').textContent = currentQuestionIndex === quizQuestions.length - 1 
        ? 'Hoàn Thành'
        : 'Câu Tiếp →';
}

function previousQuestion() {
    if (currentQuestionIndex > 0) {
        currentQuestionIndex--;
        displayQuestion();
    }
}

function nextQuestion() {
    if (currentQuestionIndex < quizQuestions.length - 1) {
        currentQuestionIndex++;
        displayQuestion();
    }
}

async function submitQuiz() {
    if (Object.keys(userAnswers).length < quizQuestions.length) {
        alert('Vui lòng trả lời tất cả các câu hỏi!');
        return;
    }

    try {
        let correctCount = 0;
        const detailedResults = [];

        for (let i = 0; i < quizQuestions.length; i++) {
            const question = quizQuestions[i];
            const userAnswer = userAnswers[i];

            const response = await fetch(`${API_BASE_URL}/quiz/submit`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    questionId: question.id,
                    answer: userAnswer
                })
            });

            const result = await response.json();
            const isCorrect = result.isCorrect;

            if (isCorrect) {
                correctCount++;
            }

            detailedResults.push({
                question: question.question,
                questionType: question.questionType,
                userAnswer: userAnswer,
                correctAnswer: question.correctAnswer,
                isCorrect: isCorrect
            });
        }

        displayResults(correctCount, detailedResults);
    } catch (error) {
        console.error('Lỗi khi nộp bài:', error);
        alert('Lỗi khi nộp bài');
    }
}

function displayResults(correctCount, detailedResults) {
    const totalCount = quizQuestions.length;
    const percentage = Math.round((correctCount / totalCount) * 100);
    const timeSpent = Math.round((new Date() - quizStartTime) / 60000);

    // Hide quiz, show results
    document.getElementById('quizContainer').style.display = 'none';
    document.getElementById('quizResults').style.display = 'block';

    // Display score
    document.getElementById('scoreValue').textContent = percentage + '%';
    document.getElementById('correctCount').textContent = correctCount;
    document.getElementById('totalCount').textContent = totalCount;
    document.getElementById('successRate').textContent = percentage + '%';
    document.getElementById('timeSpent').textContent = timeSpent;

    // Display detailed results
    const detailedResultsDiv = document.getElementById('detailedResults');
    detailedResultsDiv.innerHTML = '';

    detailedResults.forEach((result, index) => {
        const resultItem = document.createElement('div');
        resultItem.className = `result-item ${result.isCorrect ? 'correct' : 'incorrect'}`;

        resultItem.innerHTML = `
            <div class="result-question">
                Câu ${index + 1}: ${result.question}
            </div>
            <div class="result-answer">
                <div class="your-answer">
                    <strong>Câu trả lời của bạn:</strong> ${result.userAnswer}
                    ${result.isCorrect ? '✓' : '✗'}
                </div>
                ${!result.isCorrect ? `
                    <div class="correct-answer">
                        <strong>Câu trả lời đúng:</strong> ${result.correctAnswer}
                    </div>
                ` : ''}
            </div>
        `;

        detailedResultsDiv.appendChild(resultItem);
    });
}

function retakeQuiz() {
    document.getElementById('quizResults').style.display = 'none';
    document.getElementById('quizSelection').style.display = 'block';
    quizQuestions = [];
    currentQuestionIndex = 0;
    userAnswers = {};
}

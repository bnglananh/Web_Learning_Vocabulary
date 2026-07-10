const API_BASE_URL = '/api';

let exercises = [];
let currentExercise = null;
let userAnswers = {};

// Initialize
document.addEventListener('DOMContentLoaded', function() {
    loadExercises();
});

async function loadExercises() {
    try {
        const response = await fetch(`${API_BASE_URL}/passageexercises`);
        exercises = await response.json();
        displayExercises();
    } catch (error) {
        console.error('Lỗi khi tải bài tập:', error);
        alert('Lỗi khi tải bài tập');
    }
}

async function generateNewExercise() {
    try {
        const btn = document.querySelector('.section-header .btn-primary');
        const originalText = btn.innerHTML;
        btn.innerHTML = '⏳ Đang tạo...';
        btn.disabled = true;

        const response = await fetch(`${API_BASE_URL}/passageexercises/generate`, {
            method: 'POST'
        });

        if (response.ok) {
            await loadExercises();
            alert('Tạo bài tập thành công!');
        } else {
            const err = await response.json();
            alert(err.message || 'Lỗi khi tạo bài tập');
        }

        btn.innerHTML = originalText;
        btn.disabled = false;
    } catch (error) {
        console.error('Lỗi khi tạo bài tập:', error);
        alert('Lỗi khi tạo bài tập: ' + error.message);
        const btn = document.querySelector('.section-header .btn-primary');
        btn.innerHTML = '✨ Tạo Bài Tập Mới (AI)';
        btn.disabled = false;
    }
}

function displayExercises() {
    const exercisesGrid = document.getElementById('exercisesGrid');

    if (!exercises || exercises.length === 0) {
        exercisesGrid.innerHTML = `
            <div class="empty-state">
                <div class="empty-state-icon">📖</div>
                <p>Không có bài tập nào</p>
            </div>
        `;
        return;
    }

    exercisesGrid.innerHTML = exercises.map(exercise => `
        <div class="exercise-card">
            <div class="exercise-title" onclick="selectExercise(${exercise.id})" style="cursor:pointer">${exercise.title}</div>
            <div class="exercise-meta">
                <span>${exercise.blankWords ? exercise.blankWords.length : 0} từ</span>
            </div>
            <div class="exercise-preview" onclick="selectExercise(${exercise.id})" style="cursor:pointer">
                ${exercise.passageWithBlanks ? exercise.passageWithBlanks.substring(0, 100) : ''}...
            </div>
            <div style="display: flex; gap: 10px; margin-top: 15px;">
                <button class="btn btn-primary exercise-btn" style="flex: 1;" onclick="selectExercise(${exercise.id})">Bắt Đầu</button>
                <button class="btn btn-danger exercise-btn" style="background: #dc3545; border-color: #dc3545;" onclick="deleteExercise(${exercise.id}, event)">Xóa</button>
            </div>
        </div>
    `).join('');
}

async function deleteExercise(id, event) {
    if (event) event.stopPropagation();
    
    if (!confirm('Bạn có chắc chắn muốn xóa bài tập này?')) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/passageexercises/${id}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            await loadExercises();
        } else {
            alert('Lỗi khi xóa bài tập');
        }
    } catch (error) {
        console.error('Lỗi khi xóa bài tập:', error);
        alert('Lỗi khi xóa bài tập');
    }
}

function selectExercise(exerciseId) {
    const exercise = exercises.find(e => e.id === exerciseId);
    if (!exercise) return;

    currentExercise = exercise;
    userAnswers = {};

    // Hide selection, show exercise
    document.getElementById('exerciseSelection').style.display = 'none';
    document.getElementById('passageExercise').style.display = 'block';

    // Display exercise
    document.getElementById('exerciseTitle').textContent = exercise.title;

    // Display passage with blanks as functional inputs
    const passageText = document.getElementById('passageText');
    
    // Sort blankWords by position to match order in passage
    const sortedBlanks = [...exercise.blankWords].sort((a, b) => a.position - b.position);
    let blankIndex = 0;
    
    passageText.innerHTML = exercise.passageWithBlanks.replace(/_____/g, () => {
        const blankWord = sortedBlanks[blankIndex];
        blankIndex++;
        if (!blankWord) return '_____';
        return `<input type="text" id="blank-${blankWord.id}" class="blank-input blank-input-inline" data-blank-id="${blankWord.id}" placeholder="nhập từ..." autocomplete="off">`;
    });

    // Display Word Bank (English only, shuffled)
    const wordBankSection = document.getElementById('wordBankSection');
    const wordBank = document.getElementById('wordBank');
    if (exercise.blankWords && exercise.blankWords.length > 0) {
        wordBankSection.style.display = 'block';
        const shuffledWords = [...exercise.blankWords].sort(() => Math.random() - 0.5);
        wordBank.innerHTML = shuffledWords.map(w => `<span class="word-bank-item">${w.englishWord}</span>`).join('');
    } else {
        wordBankSection.style.display = 'none';
    }
}

function backToExercises() {
    document.getElementById('passageExercise').style.display = 'none';
    document.getElementById('passageResults').style.display = 'none';
    document.getElementById('exerciseSelection').style.display = 'block';
    currentExercise = null;
    userAnswers = {};
}

async function submitPassage() {
    if (!currentExercise) return;

    // Collect answers
    const answers = [];
    const inputs = document.querySelectorAll('.blank-input');

    inputs.forEach(input => {
        const blankId = parseInt(input.dataset.blankId);
        const userAnswer = input.value.trim();

        answers.push({
            blankId: blankId,
            userAnswer: userAnswer
        });
    });

    // Check if all blanks are filled
    if (answers.some(a => !a.userAnswer)) {
        alert('Vui lòng điền tất cả các chỗ trống!');
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/passageexercises/${currentExercise.id}/submit`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(answers)
        });

        const submission = await response.json();
        displayPassageResults(submission);
    } catch (error) {
        console.error('Lỗi khi nộp bài:', error);
        alert('Lỗi khi nộp bài');
    }
}

function displayPassageResults(submission) {
    // Hide exercise, show results
    document.getElementById('passageExercise').style.display = 'none';
    document.getElementById('passageResults').style.display = 'block';

    // Display score
    const score = submission.correctCount;
    const total = submission.totalCount;
    const percentage = Math.round((score / total) * 100);

    document.getElementById('passageScore').textContent = score;
    document.getElementById('passageMax').textContent = total;

    let resultMessage = '';
    let resultLevel = '';

    if (percentage === 100) {
        resultMessage = '🎉 Tuyệt vời! Bạn đã hoàn thành hoàn hảo!';
        resultLevel = 'Xuất sắc';
    } else if (percentage >= 80) {
        resultMessage = '👍 Rất tốt! Bạn đạt được điểm cao!';
        resultLevel = 'Tốt';
    } else if (percentage >= 60) {
        resultMessage = '👌 Bạn làm khá tốt! Hãy tiếp tục luyện tập!';
        resultLevel = 'Khá';
    } else {
        resultMessage = '💪 Bạn cần luyện tập thêm! Hãy cố gắng!';
        resultLevel = 'Cần cải thiện';
    }

    document.getElementById('resultMessage').textContent = resultMessage;
    document.getElementById('resultLevel').textContent = resultLevel;

    // Display detailed answers
    const answersReview = document.getElementById('answersReview');
    answersReview.innerHTML = '';

    submission.answers.forEach((answer, index) => {
        const answerItem = document.createElement('div');
        answerItem.className = `answer-item ${answer.isCorrect ? 'correct' : 'incorrect'}`;

        answerItem.innerHTML = `
            <div class="answer-item-header">
                <span class="answer-position">Chỗ trống ${index + 1}</span>
                <span class="answer-status ${answer.isCorrect ? 'correct' : 'incorrect'}">
                    ${answer.isCorrect ? '✓ Đúng' : '✗ Sai'}
                </span>
            </div>
            <div class="answer-comparison">
                <div class="answer-field">
                    <span class="answer-field-label">Câu trả lời của bạn</span>
                    <div class="answer-field-value">${answer.userAnswer}</div>
                </div>
                <div class="answer-field">
                    <span class="answer-field-label">Câu trả lời đúng</span>
                    <div class="answer-field-value">${answer.correctAnswer}</div>
                    <div class="answer-meaning">
                        ${getCurrentBlankMeaning(answer.correctAnswer)}
                    </div>
                </div>
            </div>
        `;

        answersReview.appendChild(answerItem);
    });
}

function getCurrentBlankMeaning(word) {
    if (!currentExercise) return '';
    const blankWord = currentExercise.blankWords.find(b => 
        b.englishWord.toLowerCase() === word.toLowerCase()
    );
    return blankWord ? blankWord.vietnameseMeaning : '';
}

function retakePassage() {
    document.getElementById('passageResults').style.display = 'none';
    document.getElementById('passageExercise').style.display = 'block';
    userAnswers = {};

    // Clear inputs
    document.querySelectorAll('.blank-input').forEach(input => {
        input.value = '';
        input.classList.remove('correct', 'incorrect');
    });
}

const API_BASE_URL = '/api';
let allVocabularies = [];

const vocabularyList = document.getElementById('vocabularyList');
const searchInput = document.getElementById('searchInput');
const startLearningBtn = document.querySelector('.btn-primary');

// Initialize
document.addEventListener('DOMContentLoaded', function() {
    fetchVocabularies();
    setupEventListeners();
});

async function fetchVocabularies() {
    try {
        const response = await fetch(`${API_BASE_URL}/vocabularies`);
        allVocabularies = await response.json();
        // Lấy 3 từ vựng mới nhất cho "Từ vựng hôm nay"
        const recentVocabs = allVocabularies.slice().reverse().slice(0, 3);
        renderVocabulary(recentVocabs);
    } catch (error) {
        console.error('Lỗi khi tải từ vựng:', error);
    }
}

function setupEventListeners() {
    if (searchInput) searchInput.addEventListener('input', filterVocabulary);

    if (startLearningBtn) {
        startLearningBtn.addEventListener('click', function() {
            alert('🎉 Chào mừng! Bắt đầu học từ vựng ngay bây giờ!');
            searchInput.focus();
        });
    }
}

// Render Vocabulary
function renderVocabulary(items) {
    vocabularyList.innerHTML = '';

    if (items.length === 0) {
        vocabularyList.innerHTML = '<div style="grid-column: 1/-1; text-align: center; padding: 2rem;"><p>Không tìm thấy từ vựng. Vui lòng thử lại!</p></div>';
        return;
    }

    items.forEach(vocab => {
        const card = createVocabCard(vocab);
        vocabularyList.appendChild(card);
    });
}

function createVocabCard(vocab) {
    const card = document.createElement('div');
    card.className = 'vocab-card';
    card.innerHTML = `
        <div class="vocab-header">
            <h3>${vocab.englishWord}</h3>
        </div>
        <div class="vocab-body">
            <p class="meaning"><strong>Nghĩa:</strong> ${vocab.vietnameseMeaning}</p>
            <p class="example"><strong>Ví dụ:</strong> ${vocab.example || 'Chưa có ví dụ'}</p>
            <p class="type"><strong>Loại từ:</strong> ${vocab.wordType || 'N/A'}</p>
        </div>

    `;
    return card;
}

// Filter Vocabulary
function filterVocabulary() {
    const searchTerm = searchInput.value.toLowerCase();

    const filtered = allVocabularies.filter(vocab => {
        return vocab.englishWord.toLowerCase().includes(searchTerm) || 
               vocab.vietnameseMeaning.toLowerCase().includes(searchTerm);
    });

    renderVocabulary(filtered.slice(0, 3));
}


// Add smooth scroll for navigation
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// Add animation on scroll
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver(function(entries) {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.style.animation = 'fadeIn 0.6s ease-out forwards';
        }
    });
}, observerOptions);

// Observe vocab cards
document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('.vocab-card').forEach(card => {
        observer.observe(card);
    });
});

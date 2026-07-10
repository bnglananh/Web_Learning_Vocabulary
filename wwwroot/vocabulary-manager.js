const API_BASE_URL = '/api';

let currentEditingId = null;

// Initialize
document.addEventListener('DOMContentLoaded', function() {
    loadVocabularies();
    setupEventListeners();
});

function setupEventListeners() {
    document.getElementById('searchInput').addEventListener('input', filterVocabularies);
}

// Load Vocabularies
async function loadVocabularies() {
    try {
        const response = await fetch(`${API_BASE_URL}/vocabularies`);
        const data = await response.json();
        displayVocabularies(data);
    } catch (error) {
        console.error('Lỗi khi tải từ vựng:', error);
        showToast('Lỗi khi tải dữ liệu', 'error');
    }
}

function displayVocabularies(vocabularies) {
    const vocabularyList = document.getElementById('vocabularyList');

    if (!vocabularies || vocabularies.length === 0) {
        vocabularyList.innerHTML = `
            <div class="empty-state">
                <div class="empty-state-icon">📚</div>
                <p>Chưa có từ vựng nào. Hãy thêm từ vựng mới!</p>
            </div>
        `;
        return;
    }

    vocabularyList.innerHTML = vocabularies.map(vocab => `
        <div class="vocab-card-manage">
            <div class="vocab-card-header">
                <h3>${vocab.englishWord}</h3>
            </div>
            <div class="vocab-body">
                <p class="meaning"><strong>Nghĩa:</strong> ${vocab.vietnameseMeaning}</p>
                <p class="type"><strong>Loại từ:</strong> ${vocab.wordType || 'N/A'}</p>
                <p class="example"><strong>Ví dụ:</strong> ${vocab.example}</p>
            </div>
            <div class="card-actions">
                <button class="card-btn card-btn-edit" onclick="editVocabulary(${vocab.id})">✏️ Sửa</button>
                <button class="card-btn card-btn-delete" onclick="deleteVocabulary(${vocab.id})">🗑️ Xóa</button>
            </div>
        </div>
    `).join('');
}

// Filter Vocabularies
function filterVocabularies() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();

    const cards = document.querySelectorAll('.vocab-card-manage');
    let visibleCount = 0;

    cards.forEach(card => {
        const word = card.querySelector('h3').textContent.toLowerCase();

        const matchSearch = word.includes(searchTerm);

        if (matchSearch) {
            card.style.display = '';
            visibleCount++;
        } else {
            card.style.display = 'none';
        }
    });

    if (visibleCount === 0) {
        const vocabularyList = document.getElementById('vocabularyList');
        vocabularyList.innerHTML = `
            <div class="empty-state">
                <div class="empty-state-icon">🔍</div>
                <p>Không tìm thấy từ vựng phù hợp</p>
            </div>
        `;
    }
}

// Open Add Modal
function openAddModal() {
    currentEditingId = null;
    document.getElementById('modalTitle').textContent = 'Thêm Từ Vựng Mới';
    document.getElementById('vocabularyForm').reset();
    document.getElementById('vocabularyModal').classList.add('open');
}

// Edit Vocabulary
function editVocabulary(id) {
    fetch(`${API_BASE_URL}/vocabularies/${id}`)
        .then(response => response.json())
        .then(vocab => {
            currentEditingId = id;
            document.getElementById('modalTitle').textContent = 'Sửa Từ Vựng';
            document.getElementById('englishWord').value = vocab.englishWord;
            document.getElementById('vietnameseMeaning').value = vocab.vietnameseMeaning;
            document.getElementById('wordType').value = vocab.wordType || '';
            document.getElementById('example').value = vocab.example;
            document.getElementById('vocabularyModal').classList.add('open');
        })
        .catch(error => {
            console.error('Lỗi khi tải từ vựng:', error);
            showToast('Lỗi khi tải dữ liệu', 'error');
        });
}

// Close Modal
function closeModal() {
    document.getElementById('vocabularyModal').classList.remove('open');
}

// Save Vocabulary
async function saveVocabulary(event) {
    event.preventDefault();

    const vocabulary = {
        englishWord: document.getElementById('englishWord').value,
        vietnameseMeaning: document.getElementById('vietnameseMeaning').value,
        wordType: document.getElementById('wordType').value,
        example: document.getElementById('example').value
    };

    try {
        let response;
        if (currentEditingId) {
            response = await fetch(`${API_BASE_URL}/vocabularies/${currentEditingId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(vocabulary)
            });
            showToast('Cập nhật từ vựng thành công!', 'success');
        } else {
            response = await fetch(`${API_BASE_URL}/vocabularies`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(vocabulary)
            });
            showToast('Thêm từ vựng thành công!', 'success');
        }

        if (response.ok) {
            closeModal();
            loadVocabularies();
        } else {
            showToast('Lỗi khi lưu từ vựng', 'error');
        }
    } catch (error) {
        console.error('Lỗi:', error);
        showToast('Lỗi khi lưu từ vựng', 'error');
    }
}

// Delete Vocabulary
async function deleteVocabulary(id) {
    if (confirm('Bạn có chắc chắn muốn xóa từ vựng này?')) {
        try {
            const response = await fetch(`${API_BASE_URL}/vocabularies/${id}`, {
                method: 'DELETE'
            });

            if (response.ok) {
                showToast('Xóa từ vựng thành công!', 'success');
                loadVocabularies();
            } else {
                showToast('Lỗi khi xóa từ vựng', 'error');
            }
        } catch (error) {
            console.error('Lỗi:', error);
            showToast('Lỗi khi xóa từ vựng', 'error');
        }
    }
}

// Toast Notification
function showToast(message, type = 'success') {
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.textContent = message;
    document.body.appendChild(toast);

    setTimeout(() => {
        toast.remove();
    }, 3000);
}

// Close modal when clicking outside
window.onclick = function(event) {
    const modal = document.getElementById('vocabularyModal');
    if (event.target == modal) {
        closeModal();
    }
}

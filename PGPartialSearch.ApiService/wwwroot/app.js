let currentPage = 1;
let currentQuery = '';
const pageSize = 20;

document.getElementById('searchBtn').addEventListener('click', () => performSearch(1));
document.getElementById('searchInput').addEventListener('keypress', (e) => {
    if (e.key === 'Enter') {
        performSearch(1);
    }
});

async function performSearch(page) {
    const query = document.getElementById('searchInput').value.trim();
    
    if (!query) {
        alert('Please enter a search term');
        return;
    }

    currentPage = page;
    currentQuery = query;

    showLoading();
    hideResults();

    try {
        const response = await fetch(`/api/search?query=${encodeURIComponent(query)}&page=${page}&pageSize=${pageSize}`);
        
        if (!response.ok) {
            throw new Error('Search failed');
        }

        const data = await response.json();
        displayResults(data);
    } catch (error) {
        console.error('Error:', error);
        alert('An error occurred while searching. Please try again.');
    } finally {
        hideLoading();
    }
}

function showLoading() {
    document.getElementById('loading').style.display = 'block';
}

function hideLoading() {
    document.getElementById('loading').style.display = 'none';
}

function hideResults() {
    document.getElementById('resultsHeader').style.display = 'none';
    document.getElementById('resultsTable').style.display = 'none';
    document.getElementById('pagination').style.display = 'none';
    document.getElementById('noResults').style.display = 'none';
}

function displayResults(data) {
    if (!data.items || data.items.length === 0) {
        document.getElementById('noResults').style.display = 'block';
        return;
    }

    // Show header
    document.getElementById('resultsHeader').style.display = 'block';
    document.getElementById('resultsCount').textContent = 
        `Found ${data.totalCount} result${data.totalCount !== 1 ? 's' : ''} (showing page ${data.page} of ${data.totalPages})`;

    // Show table
    document.getElementById('resultsTable').style.display = 'table';
    const tbody = document.getElementById('resultsBody');
    tbody.innerHTML = '';

    const startIndex = (data.page - 1) * pageSize;
    data.items.forEach((person, index) => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${startIndex + index + 1}</td>
            <td><strong>${escapeHtml(person.firstName)}</strong></td>
            <td><strong>${escapeHtml(person.lastName)}</strong></td>
            <td>${escapeHtml(person.email)}</td>
            <td>${escapeHtml(person.phone)}</td>
            <td>${escapeHtml(person.city)}</td>
        `;
        tbody.appendChild(row);
    });

    // Show pagination
    displayPagination(data.page, data.totalPages);
}

function displayPagination(currentPage, totalPages) {
    const pagination = document.getElementById('pagination');
    pagination.innerHTML = '';
    pagination.style.display = 'flex';

    // Previous button
    const prevBtn = document.createElement('button');
    prevBtn.textContent = '← Previous';
    prevBtn.disabled = currentPage === 1;
    prevBtn.addEventListener('click', () => performSearch(currentPage - 1));
    pagination.appendChild(prevBtn);

    // Page info
    const pageInfo = document.createElement('span');
    pageInfo.textContent = `Page ${currentPage} of ${totalPages}`;
    pageInfo.style.display = 'flex';
    pageInfo.style.alignItems = 'center';
    pageInfo.style.padding = '0 20px';
    pageInfo.style.fontWeight = '600';
    pagination.appendChild(pageInfo);

    // Next button
    const nextBtn = document.createElement('button');
    nextBtn.textContent = 'Next →';
    nextBtn.disabled = currentPage === totalPages;
    nextBtn.addEventListener('click', () => performSearch(currentPage + 1));
    pagination.appendChild(nextBtn);
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

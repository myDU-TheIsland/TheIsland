// Homepage Tab js
function showTab(tabId) {
  // Hide all tabs
  document
    .querySelectorAll('.tab-content')
    .forEach((tab) => tab.classList.remove('active-tab'));
  document
    .querySelectorAll('.tab-button')
    .forEach((button) => button.classList.remove('active-tab-button'));

  // Show the selected tab
  document.getElementById(tabId).classList.add('active-tab');
  document
    .querySelector(`[onclick="showTab('${tabId}')"]`)
    .classList.add('active-tab-button');
}

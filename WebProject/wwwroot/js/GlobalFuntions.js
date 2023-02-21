//Delete session storage after 5 minutes.
let SessionStorageExpireTimer = setTimeout(() => {
	sessionStorage.clear();
}, 300000);

function CancelSessionStorageExpireTimer() {
	if (SessionStorageExpireTimer !== undefined)
		clearTimeout(SessionStorageExpireTimer)
}

function DeleteSessionStorage() {
	sessionStorage.clear();
	clearTimeout(SessionStorageExpireTimer)
}

function IsString(string) {
	if (typeof string === 'string')
		return true;

	return false;
}

function Message(message) {
	if (!IsString(message))
		return;

	const messageContainer = document.createElement('aside');
	messageContainer.className = 'alert-message';

	messageContainer.innerHTML = message;

	const deleteButton = document.createElement('button');
	deleteButton.className = 'close-button';
	deleteButton.onclick = function () {
		this.parentElement.style.display = 'none';
	}
	messageContainer.appendChild(deleteButton);

	document.getElementById('main-header').appendChild(messageContainer);
}
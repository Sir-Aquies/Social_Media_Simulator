//Main container of all the posts.
const mainContainer = document.getElementById('UserPostContainer');
//Array that will contain all of the posts.
const postContainers = [...document.getElementsByClassName('post-container')];

function AddPostToContainer(postString) {
    const post = ConvertToDOM(postString);

    //Insert the post before the create post button.
    mainContainer.insertBefore(post, mainContainer.children[1]);
}

function RemovePostFromContainer(postId) {
    const arr = window.location.href.split('/');
    const userName = arr[arr.length - 1];

    const post = postContainers.find(p => p.id == `${userName}${postId}`);
    post.remove();
}

function UpdatePostFromContainer(postString) {
    const post = ConvertToDOM(postString);
    //Find the old post in the array.
    const oldPost = postContainers.find(p => p.id == post.id);

    //Insert new post before the old post.
    mainContainer.insertBefore(post, oldPost);

    //Remove old post.
    oldPost.remove();
}

//This function converts a string in to a DOM and returns the post.
function ConvertToDOM(htmlString) {
    return new DOMParser().parseFromString(htmlString, 'text/html').all[3];
}
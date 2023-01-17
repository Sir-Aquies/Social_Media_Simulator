﻿//Main container of all the posts.
const mainContainer = document.getElementById('UserPostContainer');
//Array that will contain all of the posts.
const posts = [...document.getElementsByClassName('post-container')];

//Array that will contain all of the posts containers.
const containers = [...document.querySelectorAll('[data-post-container]')];

function AddPostToContainer(postString) {
    //Convert the string post into an object element.
    const newPost = ConvertToDOM(postString);

    //Create a container for the post.
    const postContainer = document.createElement('article');

    //Set the id.
    postContainer.id = `${newPost.id}`;

    //Add the data attribute data-post-container.
    postContainer.dataset.postContainer = '';

    //Add the onclick event to redirect to CompletePost.
    postContainer.onclick = () => {
        location = `${newPost.dataset.username}/hop/${newPost.dataset.id}`;
    }
    postContainer.appendChild(newPost);

    //Insert the container after the create post button.
    mainContainer.insertBefore(postContainer, mainContainer.children[1]);

    //Push the new post and his container to the arrays.
    posts.push(newPost);
    containers.push(postContainer);
}

function RemovePostFromContainer(postId) {
    //Find the post container base on his id and remove it.
    const postContainer = containers.find(p => p.id == `${postId}`);
    postContainer.remove();

    //Function only defined in CompletePost that redirects the user to his page.
    RedirectToUserPage();
}

function UpdatePostFromContainer(postString) {
    //Convert the string post into an HTML element.
    const updatedPost = ConvertToDOM(postString);

    //Find the outdated post's container in containers.
    const outdatedPostContainer = containers.find(p => p.id == `${updatedPost.dataset.id}`);

    //Get the outdated post an remove it.
    const outdatedPost = outdatedPostContainer.children[0];
    outdatedPost.remove();

    //Add the updated post.
    outdatedPostContainer.prepend(updatedPost);
}

function AddCommentToPost(commentString) {
    //Convert the string comment into an object element.
    const newComment = ConvertToDOM(commentString);

    //Find the post container from where the comment belongs.
    const postContainer = containers.find(p => p.id == `${newComment.dataset.postid}`);

    //Insert the new comment right after the post.
    postContainer.insertBefore(newComment, postContainer.children[1]);

    //Get the span element that holds the amount of comments and increase it.
    const commentSpan = document.getElementById(`Comments-${newComment.dataset.postid}`);
    let commentAmount = parseInt(commentSpan.innerHTML);
    commentSpan.innerHTML = `${++commentAmount}`;
}

function RemoveCommentFromPost(postId, commentId) {
    //Find to post container.
    const postContainer = containers.find(p => p.id == `${postId}`)

    //Find the comment by the id and remove it.
    for (let i = 0; i < postContainer.children.length; i++) {
        if (parseInt(postContainer.children[i].dataset.id) === commentId) {
            postContainer.children[i].remove();
        }
    }

    //Get the span element that holds the amount of comments and decrease it.
    const commentSpan = document.getElementById(`Comments-${postId}`);
    let commentAmount = parseInt(commentSpan.innerHTML);
    commentSpan.innerHTML = `${--commentAmount}`;
}

//This function converts a string in to a DOM and returns the element.
function ConvertToDOM(htmlString) {
    return new DOMParser().parseFromString(htmlString, 'text/html').all[3];
}
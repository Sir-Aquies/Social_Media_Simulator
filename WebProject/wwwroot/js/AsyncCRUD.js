//Main container of all the posts.
const mainContainer = document.getElementById('UserPostContainer');
//Array that will contain all of the posts.
const posts = [...document.getElementsByClassName('post-container')];

//Array that will contain all of the posts containers.
const postContainers = [...document.querySelectorAll('[data-post-container]')];

function AddPostToContainer(postString) {
    //Convert the string post into an object element.
    const newPost = ConvertToDOM(postString);

    //Create a container for the post.
    const postContainer = document.createElement('article');
    //Set the id, a combinatio of username and id.
    postContainer.id = `${newPost.dataset.username}${newPost.id}`;
    //Add the data attribute data-post-container.
    postContainer.dataset.postContainer = '';

    //Add the onclick event to redirect to CompletePost.
    postContainer.onclick = () => {
        location = `${newPost.dataset.username}/hop/${newPost.id}`;
    }
    postContainer.appendChild(newPost);

    //Insert the container before the create post button.
    mainContainer.insertBefore(postContainer, mainContainer.children[1]);

    //Push the new post and his container to the arrays.
    posts.push(newPost);
    postContainers.push(postContainer);
}

function RemovePostFromContainer(postId, userName) {
    //Find the post container base on his id and post's username and remove it.
    const postContainer = postContainers.find(p => p.id == `${userName}${postId}`);
    postContainer.remove();

    //Function only defined in CompletePost that redirects the user to his page.
    RedirectToUserPage();
}

function UpdatePostFromContainer(postString) {
    //Convert the string post into an HTML element.
    const updatedPost = ConvertToDOM(postString);

    //Find the outdated post's container in postContainers.
    const outdatedPostContainer = postContainers.find(p => p.id == `${updatedPost.dataset.username}${updatedPost.id}`);

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
    const postContainer = postContainers.find(p => p.id == `${newComment.dataset.username}${newComment.dataset.postid}`);

    //Insert the new comment right after the post.
    postContainer.insertBefore(newComment, postContainer.children[1]);
}

function RemoveCommentFromPost(postId, userName, commentId) {
    //Find to post container.
    const postContainer = postContainers.find(p => p.id == `${userName}${postId}`)

    //Find the comment by the id and remove it.
    for (let i = 0; i < postContainer.children.length; i++) {
        if (parseInt(postContainer.children[i].id) === commentId) {
            postContainer.children[i].remove();
        }
    }
}

//This function converts a string in to a DOM and returns the element.
function ConvertToDOM(htmlString) {
    return new DOMParser().parseFromString(htmlString, 'text/html').all[3];
}
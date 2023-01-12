//Main container of all the posts.
const mainContainer = document.getElementById('UserPostContainer');
//Array that will contain all of the posts.
const posts = [...document.getElementsByClassName('post-container')];

const postContainers = [...document.querySelectorAll('[data-post-container]')];

function AddPostToContainer(postString) {
    //Convert the string post into an object element.
    const newPost = ConvertToDOM(postString);

    //Create a container for the post.
    const article = document.createElement('article');
    //TODO -  add metadata to the article and change name.
    article.appendChild(newPost);

    //Insert the container before the create post button.
    mainContainer.insertBefore(article, mainContainer.children[1]);

    posts = [...document.getElementsByClassName('post-container')];
    postContainers = [...document.querySelectorAll('[data-post-container]')];
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

function AddCommentToPost(commentString, postId) {
    //Convert the string comment into an object element.
    const newComment = ConvertToDOM(commentString);
    //Find the post where the comment belongs.
    const postToAdd = posts.find(p => p.id == postId);



}

//This function converts a string in to a DOM and returns the element.
function ConvertToDOM(htmlString) {
    return new DOMParser().parseFromString(htmlString, 'text/html').all[3];
}
//Main container of all the posts.
const mainContainer = document.getElementById('UserPostContainer');
//Array that will contain all of the posts.
const posts = [...document.getElementsByClassName('post-container')];

//Array that will contain all of the posts containers.
const containers = [...document.querySelectorAll('[data-post-container]')];

let loadingPosts = false;

function SetScrollEvent(userId, startFrom, PostLoader) {
    let startFromRow = parseInt(startFrom);
    let amountOfRows = 5;

    window.onscroll = function() {
        if (!loadingPosts && this.window.scrollY > (mainContainer.clientHeight * (70 / 100))) {
            loadingPosts = true;
            PostLoader(userId, startFromRow, amountOfRows);
            startFromRow += amountOfRows;
        }
    }
}

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
    mainContainer.insertBefore(postContainer, mainContainer.children[2]);

    //Push the new post and his container to the arrays.
    posts.push(newPost);
    containers.push(postContainer);
}

function AddRangePost(postsString) {
    const newPosts = new DOMParser().parseFromString(postsString, 'text/html').all[2].children;
    let length = newPosts.length;

    for (let i = 0; i < length; i++) {
        //Append post to the main container (center-box class) and push it in the containers array.
        containers.push(newPosts[0]);
        mainContainer.append(newPosts[0]);
    }
}

function AlterPostLikes(postId, action) {
    const likesAmountSpans = [...document.querySelectorAll(`[data-post-likes-${postId}]`)];

    for (let i = 0; i < likesAmountSpans.length; i++) {
        //Get the span element and increase/decrease the amount of likes.
        const likesSpan = likesAmountSpans[i].children[0];
        let likes = parseInt(likesSpan.innerHTML);

        if (action === '+')
            likesSpan.innerHTML = ++likes;

        if (action === '-')
            likesSpan.innerHTML = --likes;

        //Get the svg element and add/delete the class "liked" to it so it looks liked/disliked.
        const SVGLikes = likesAmountSpans[i].children[1];
        if (action === '+')
            SVGLikes.className.baseVal = 'like-button liked';

        if (action === '-')
            SVGLikes.className.baseVal = 'like-button';

        //Chage the font weight of the button and change its title.
        if (action === '+') {
            likesAmountSpans[i].style.fontWeight = 'bold';
            likesAmountSpans[i].title = 'Dislike post';
        }

        if (action === '-') {
            likesAmountSpans[i].style.fontWeight = 'normal';
            likesAmountSpans[i].title = 'Like post';
        }
    }
}

function RemovePostFromContainer(postId) {
    //Find the post container base on his id and remove it.
    const postContainer = containers.find(p => p.id == `${postId}`);
    postContainer.remove();

    //Get the index an remove it from containers;
    let index = containers.findIndex(p => p.id == `${postId}`);
    containers.splice(index, 1);

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

    //If view post tab is open also update the post there.
    const viewPost = document.getElementById(`view-post-${updatedPost.dataset.id}`);
    if (viewPost) {
        //Get the outdated post an remove it.
        const outdatedPost = viewPost.children[0];
        outdatedPost.remove();

        //Add the updated post.
        viewPost.prepend(updatedPost.cloneNode(true));
    }
}

function AddCommentToPost(commentString) {
    //Convert the string comment into an object element.
    const newComment = ConvertToDOM(commentString);

    //Find the post container from where the comment belongs.
    const postContainer = containers.find(p => p.id == `${newComment.dataset.postid}`);

    //Insert the new comment right after the post.
    postContainer.insertBefore(newComment, postContainer.children[1]);

    //Get the spans elements (there can be 2 if view post tab is open) that holds the amount of comments and increase it.
    const commentsButtons = [...document.querySelectorAll(`[data-post-comments-${newComment.dataset.postid}]`)];
    for (let i = 0; i < commentsButtons.length; i++) {
        const commentsAmountSpan = commentsButtons[i].children[0];
        let amountOfComments = parseInt(commentsAmountSpan.innerHTML);
        commentsAmountSpan.innerHTML = ++amountOfComments;
    }

    //If the user has the the view post tab open also add the comment there.
    const viewPost = document.getElementById(`view-post-${newComment.dataset.postid}`);

    if (viewPost) {
        viewPost.insertBefore(newComment.cloneNode(true), viewPost.children[1]);
    }
}

function AlterCommentLikes(commentId, action) {
    const likesAmountSpans = [...document.querySelectorAll(`[data-comment-likes-${commentId}]`)];

    for (let i = 0; i < likesAmountSpans.length; i++) {
        //Get the span element and increase/decrease the amount of likes.
        const likesSpan = likesAmountSpans[i].children[0];
        let likes = parseInt(likesSpan.innerHTML);

        if (action === '+')
            likesSpan.innerHTML = ++likes;

        if (action === '-')
            likesSpan.innerHTML = --likes;

        //Get the svg element and add/delete the class "liked" to it so it looks liked/disliked.
        const SVGLikes = likesAmountSpans[i].children[1];
        if (action === '+')
            SVGLikes.className.baseVal = 'like-button liked';

        if (action === '-')
            SVGLikes.className.baseVal = 'like-button';

        //Chage the font weight of the button and change its title.
        if (action === '+') {
            likesAmountSpans[i].style.fontWeight = 'bold';
            likesAmountSpans[i].title = 'Disliked comment';
        }

        if (action === '-') {
            likesAmountSpans[i].style.fontWeight = 'normal';
            likesAmountSpans[i].title = 'Like comment';
        }
    }
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

    //Get the spans elements (there can be 2 if view post tab is open) that hold the amount of comments and decrease it.
    const commentsButtons = [...document.querySelectorAll(`[data-post-comments-${postId}]`)];
    for (let i = 0; i < commentsButtons.length; i++) {
        const commentsAmountSpan = commentsButtons[i].children[0];
        let amountOfComments = parseInt(commentsAmountSpan.innerHTML);
        commentsAmountSpan.innerHTML = --amountOfComments;
    }

    //If view post tab is open also remove the comment from there.
    const viewPost = document.getElementById(`view-post-${postId}`);
    if (viewPost) {
        for (let i = 0; i < viewPost.children.length; i++) {
            if (parseInt(viewPost.children[i].dataset.id) === commentId) {
                viewPost.children[i].remove();
            }
        }
    }
}

function EmptyMainContainer() {
    let length = containers.length;

    for (let i = 0; i < length; i++) {
        containers[i].remove();
    }

    containers.splice(0, containers.length)
}

//This function converts a string in to a DOM and returns the element.
function ConvertToDOM(htmlString) {
    return new DOMParser().parseFromString(htmlString, 'text/html').all[3];
}
# WebProject
ASP .NET core project

<h2>Models</h2>

### UserModel

UserModel as the name suggest is the model for the users, it holds the personal information of the user, it is create at the register page, it can be edited at the    settings page page and it can be "eliminated" at the settings page.

Properties:
The properties type in C# and date type in SQL server.
- Username (string, nvarchar(150)).
- FirstName (string, nvarchar(200)).
- LastName (string, nvarchar(200)).
- DateofBirth (date, datetime2(7)).
- Email (string, nvarchar(200)).
- Password (string, nvarchar(150)).
- Description (string, nvarchar(500)).
- ProfilePicture (image, image).
- Posts (`List<PostModel>`, Table).
- FavoritesPost (`List<PostModel>`, Table).

### PostModel

Users can make post which contain text and media (jpg, gif, videos), also other users can like the post, comment about the post and add the post to thier favorites.

Properties:
The properties type in C# and date type in SQL server.

* Content (string, nvarchar(MAX)).
* Media (Image-Video, image)
* Likes (int, int).
* Comments (`List<CommentModel>`, Table).

### CommentModel

Comments can be written by users on posts. Comments can have replies.

Properties:
The properties type in C# and date type in SQL server.

* Content (string, nvarchar(MAX))
* Likes (int, int)
* Replies (List<ReplyModel>, Table)

### ReplyModel

Replies can be written in comments. These replies won't include images (maybe).

Properties:
The properties type in C# and date type in SQL server.

* Content (string, nvarchar(MAX)).
* Likes (int, int).

## Pages

### Login Page

The login page is pretty simple, it will be the page where users can log in into their accounts. The page will ask for username/email and password. There will also be a link to the register page. This page may end up being the index or maybe the explore page without an account. 

### Register Page

This page is very straight forward, it will ask for email, username, first name, last name, date of birth, password and confirm password. There can not be blank spaces in the fields, usernames and emails will be unique and password need to be more than 8 characters or longer.

### Home Page

The home page will display the user's posts, as well as, the user's profile picture, username, full name and description. This will also be the page where the user can make post, edit previous post, etc.

### Profile Page 

The Profile page is mean for other user's profiles when you click in their profile picture or username in comments or in the explore page, it will dislay the user's username, profile picture, posts, etc. If the user clicks in his own profile picture it will take him to the home page.

### Explore Page

This page will show other user's posts. This page is the most experimental one for me, because i wanted to learn use an algorithm but if i use one i will have to make a lot of fake post (which i may not want to, i that case i can just make a few post and show them randomly).

### Settings Page

The setting page will show some settings like darkmode and edit profile.

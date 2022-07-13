# WebProject
ASP .NET core project


<h2>Models</h2>

### UserModel

UserModel as the name suggest is the model for the users, it holds the personal information of the user, it is create at the Register page, it can be edited at the    EditProfile page and it can be "eliminated" at the Settings page.

Properties:
The properties type in C# and date type in SQL server.
- Username (string, nvarchar(150))
- Firstname (string, nvarchar(200))
- Lastname (string, nvarchar(200))
- DateofBirth (date, datetime2(7))
- Email (string, nvarchar(200))
- Password (string, nvarchar(150))
- Description (string, nvarchar(500))
- ProfilePicture (image, image)
- Posts (`List<PostModel>`, Table)
- FavoritesPost (`List<PostModel>`, Table)

### PostModel

Users can make post which contain text and media (jpg, gif, videos), also other users can like the post, comment about the post and add the post to thier favorites.

Properties:
The properties type in C# and date type in SQL server.
* Content (string, nvarchar(MAX))
* Media (Image-Video, image)
* Likes (int, int)
* Comments (`List<CommentModel>`, Table)

CommentModel {
Content (string, nvarchar(MAX))
Likes (int, int)
Replies (List<ReplyModel>, Table)
}

Pages {
Login
Register
Home
Profile
EditProfile
Settings
}

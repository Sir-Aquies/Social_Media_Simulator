$post("GetUsers", function(data, status){
	if (status === "success") {
		$("#UsersTable").html(data);
	}
});
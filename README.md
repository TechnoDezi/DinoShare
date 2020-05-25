# DinoShare
C# .Net core web application that can be locally hosted to share your personal or enterprise files with external user. Turn your PC or server into a personal private cloud.

A fully on-premises solution to manage and share your files.

Set Read-only, edit or delete permissions and only share files with authorized users.

DinoShare makes use of Virtual folders. Each folder can be assigned one or more system directories, thus bringing together files from various places into one container. Users can be granted access to a folder.

Dino share was created out of pure frustration with other tools out there. It either costs money, or it can only be hosted on linux. And the biggest problem is that most tools have a limited file upload size of a couple 100Mb. Dino share makes use of file chunking thus allowing virtually any file size to be uploaded.

DinoShare can be hosted on windows, linux and mac as it is written in pure .net core. Host it on it's own, behind IIS or NGINX, it's up to you.

Currently DinoShare makes use of Entity Framework core and MSSql database. This can easily be swapped out to use any of the EF database providers sych as MySql or Sql Lite. This makes it flexible to work as a personal or private cloud or in an enterprize environment.

Use the software for free, modify it as you wish no license. All I ask is that you don't sell it as a service, and if you do to please donate some of the income back to me, we all have to eat.

document.addEventListener("DOMContentLoaded", function(){
    const navbar_links = document.querySelectorAll(".nav-link");
    console.log("File _header.js đã chạy sau khi DOM được load!");
    navbar_links.forEach(link => {
        link.addEventListener("click", function () {
            navbar_links.forEach(nav => nav.classList.remove("active"));
            this.classList.add("active");
        });
    });
});
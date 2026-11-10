const showUploadButton = document.getElementById("showUploadButton");

const uploadSection = document.getElementById("uploadSection");

const searchInput = document.getElementById("searchInput");

const videoList = document.getElementById("videoList");

const uploadForm = document.getElementById("uploadForm");

// So that we dont bombard our server with search request
//let searchTimeout; 

let videos = [];

showUploadButton.addEventListener("click", () => {
    uploadSection.hidden = !uploadSection.hidden;

    if (uploadSection.hidden) 
    {
        showUploadButton.textContent = "Upload video";
    } 
    else 
    {
        showUploadButton.textContent = "Cancel";
    }
});


/* This is frontend searching*/
searchInput.addEventListener("input", () => {
    const searchText =
        searchInput.value.toLowerCase();

    const filteredVideos = videos.filter(video => {
        return video.title.toLowerCase().includes(searchText)
            || video.genre.toLowerCase().includes(searchText);
    });

    renderVideos(filteredVideos);
});

// frontend LoadVideos
async function loadVideos()
{
    const response = await fetch("/videos");

    if (!response.ok)
    {
        console.error("Could not load videos:", response.status);
        return;
    }

    videos = await response.json();

    renderVideos(videos);
}


/*
//This is backend searching
searchInput.addEventListener("input", async () =>
{
    clearTimeout(searchTimeout);
    
    searchTimeout = setTimeout(async () => 
    {
        const searchText = searchInput.value;
        await loadVideos(searchText);
    }, 300)
    //300 ms debounce

    
});

 */

//Backend loadVideos function
/*
async function loadVideos(searchText = "")
{
    let url = "/videos";

    if (searchText.trim() !== "")
    {
        url = `/videos?search=${encodeURIComponent(searchText)}`;
        //encodeURIComponent converts text like Action & Comedy into Action%20%26%20Comedy that URI can use
        //si that the URL does not interpret "&" as an action it needs to take
    }

    const response = await fetch(url);

    if (!response.ok)
    {
        console.error("Could not load videos:", response.status);

        return;
    }

    videos = await response.json();

    renderVideos(videos);
}
*/

function renderVideos(videoArray) {
    videoList.innerHTML = ""; 

    videoArray.forEach(video => {
        const videoElement = document.createElement("div");
        videoElement.classList.add("video-card");

        videoElement.innerHTML = `
            <h4 class="video-title">${video.title}</h4>

            <div class="video-content">
                <p>Genre: ${video.genre}</p>

                <video
                 controls
                 src="/videos/${video.id}/stream">
                </video>
                
                <button class="delete-video-button">
                    Delete
                </button>
            </div>
        `;

        const deleteButton = videoElement.querySelector(".delete-video-button");
        
        deleteButton.addEventListener("click", async () =>
        {
            const response = await fetch(`/videos/${video.id}`,
                {
                    method: "DELETE"
                });

            if (!response.ok)
            {
                console.error("Delete failed:", response.status);
                return;
            }
            
            await loadVideos(); //frontend
            // loadVideos(searchInput.value); //backend
        });
        
        videoList.appendChild(videoElement);
    });
}

uploadForm.addEventListener("submit", async (event) =>
{
    event.preventDefault();

    const formData = new FormData(uploadForm);

    const response = await fetch("/videos",
        {
            method: "POST",
            body: formData
        });

    if (!response.ok)
    {
        console.error("Upload failed:", response.status);
        return;
    }

    uploadForm.reset();

    uploadSection.hidden = true;
    showUploadButton.textContent = "Upload video";

    await loadVideos(); //frontend
    //await loadVideos(searchInput.value); //backend
});


loadVideos();


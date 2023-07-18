# NOTES:

Uploading images to Cloudinary and saving references to Mongo:

upload -> https://localhost:5001/api/images?filename=test-image&folder=test-api-folder/test-images&max-width=2400&widths=720,1280,1920&q=70

The server expects an Api key passed in headers as ApiKey; It's value is defined in the .env file and picked up automatically.

Ports: 
- Production: 5000;
- Development: 5000;


Requires the following environment variables:
- `CLOUDINARY_URL` - Cloudinary secret
- `CLOUDINARY_SECRET` - Cloudinary secret
- `CLOUDINARY_URL` - cloudinary://`${CLOUDINARY_KEY}``:`${CLOUDINARY_SECRET}`@`${CLOUDINARY_NAME}`
- `CLOUDINARY_NAME` - your Cloudinary username
- `API_KEY` - a header used to access the API
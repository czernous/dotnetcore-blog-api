# NOTES:

This API depends on **System.Drawing.Common**. However, in version 6, it only works under Windows.
DO NOT UPDATE.

Uploading images to Cloudinary and saving references to Mongo:

upload -> https://localhost:5001/api/images?filename=test-image&folder=test-api-folder/test-images&max-width=2400&widths=720,1280,1920&q=70

The server expects an Api key passed in headers as ApiKey; It's value is defined in the .env file and picked up automatically.

Ports: 
- Production: 5002;
- Development: 5000(http)/5001(https);

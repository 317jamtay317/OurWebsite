# ==========================================================================
# ProManager Online — static marketing site.
# Served by nginx. No build step: the site is plain HTML/CSS/JS.
#
#   docker compose up --build      # build + run at http://localhost:8080
#   docker build -t pmo-site .     # build the image manually
#   docker run -p 8080:80 pmo-site # run the image manually
# ==========================================================================
FROM nginx:1.27-alpine

# Use our server configuration (clean URLs, gzip, security headers, 404).
COPY default.conf /etc/nginx/conf.d/default.conf

# Copy the static site into nginx's web root.
COPY public/ /usr/share/nginx/html/

EXPOSE 80

# Report container health by fetching the home page.
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget -q -O /dev/null http://localhost/ || exit 1

# nginx:alpine already starts nginx in the foreground by default.

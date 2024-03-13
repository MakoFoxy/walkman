docker build -t registry.gitlab.com/salad-crew/walkman/publisher:develop -f ../Backend/Player.Publisher/Dockerfile ../Backend/
docker push registry.gitlab.com/salad-crew/walkman/publisher:develop
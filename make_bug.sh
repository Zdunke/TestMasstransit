#!/bin/bash

if [[ "${#@}" == "0" ]]
then
    echo "./make_bug.sh [address of docker host]"
    exit 1
fi

DOCKER_ADDRESS=$1
RABBITMQ_UI=${DOCKER_ADDRESS}:15672

function getchannels(){
    curl --fail $RABBITMQ_UI/api/channels -u admin:admin 2> /dev/null
}

function are_there_channels(){
    local result=$(getchannels)
    local exit_code=$?
    if [[ "$result" != "[]" ]] || [[ "$exit_code" != "0" ]]
    then
        #echo $result
        #echo $exit_code
        exit_code=1
    fi
    
    return $exit_code
}

# checks if there are no channels
# 100 times with delay in 0.1 second
function check_if_there_are_channels(){
    local i=60
    echo Checking if there are no channels
    while [[ "$i" != '0' ]]
    do
        if ! are_there_channels
        then
            echo There are existed channels
            return 1
        fi
        sleep 1s
        (( i-- ))
    done
    
    echo There are no channels
    return 0
}

function is_partition_not_detected(){
    docker exec -it $1 /bin/bash -c "rabbitmqctl cluster_status 2> /dev/null | grep 'partitions,\[\]' &> /dev/null"
}

function docker_kill(){
    echo Killing $1...
    
    (docker kill $1) > /dev/null
    
    echo Killed
}

function docker_start(){
    echo Starting $1...
    
    (docker start $1) > /dev/null
    
    echo Started
}

function wait_rabbitmq(){
    echo Waiting for rabbitmq...
    while ! curl --fail $RABBITMQ_UI &> /dev/null
    do
        sleep 0.1s
    done
    
    echo RabbitMQ is up
}

# Restarts RabbitMQ cluster 5 times
function restart_rabbitmqs(){
    local i=1
    local c=0
    local rabbits=( "rabbitmq1" "rabbitmq2" "rabbitmq3" )
    while [[ "$i" != '0' ]]
    do
        echo Restarting RabbitMQs. Times left: $i
        if connected_to=$(getchannels | egrep -oh 'rabbitmq[[:digit:]]+')
        then
            docker_kill ${rabbits[$c + 1]}
            docker_kill $connected_to
            docker_start ${rabbits[$c + 1]}
            docker_start $connected_to
        else
            docker_kill ${rabbits[$c + 1]}
            docker_kill ${rabbits[$c]}
            docker_start ${rabbits[$c + 1]}
            docker_start ${rabbits[$c]}
        fi
        echo "Wait when rabbitmq will be up..."
        while ! is_partition_not_detected rabbitmq1 && ! is_partition_not_detected rabbitmq2 && ! is_partition_not_detected rabbitmq3
        do
            sleep 1s
        done
        # make more mess
        docker_kill haproxy
        sleep 10s
        docker_start haproxy
        (( i-- ))
        ((c ^= 1))
        echo RabbitMQ were restarted
    done
    
    return 0
}

while ! check_if_there_are_channels
do
    restart_rabbitmqs
done
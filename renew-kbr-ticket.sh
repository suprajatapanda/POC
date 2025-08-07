#!/bin/sh

pre_check() {
    if [ -z "${BEND_FW_USER}" ] || [ -z "${BEND_FW_PWD}" ]; then
        echo "Error: Environment variables BEND_FW_USER and BEND_FW_PWD must be set."
        exit 99
    fi
}

authenticate() {
   # echo "\$9chOUzp+R" | kinit "svctart@US.AEGON.COM"
     echo "${BEND_FW_PWD}" | kinit "${BEND_FW_USER}"
}

authenticate_with_retry() {
    authenticate
    local status=$?
    if [ $status -ne 0 ]; then
        echo "kinit failed, attempting kdestroy and retry..."
        kdestroy
        authenticate
        status=$?
        if [ $status -ne 0 ]; then
            echo "kinit failed again, exiting with status $status."
            exit $status
        fi
    fi
}
validate_kerberos() {
    if klist -s; then
        echo "Post authentication validation succeeded."
        exit 0
    else
        echo "Post authentication validation failed."
        exit 98
    fi
}

pre_check
authenticate_with_retry
validate_kerberos


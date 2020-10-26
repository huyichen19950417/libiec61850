import iec61850

def main():
    con = iec61850.IedConnection_create()
    err = iec61850.IedConnection_connect(con, "192.168.1.41", 102)
    
    [deviceList, err] = iec61850.IedConnection_getLogicalDeviceList(con)
    device = iec61850.LinkedList_getNext(deviceList)

    while device:
        print("LD: {}".format(iec61850.toCharP(device.data)))
        [LN, err] = iec61850.IedConnection_getLogicalDeviceDirectory(con, iec61850.toCharP(device.data))
        device = iec61850.LinkedList_getNext(device)
    
    iec61850.LinkedList_destroy(deviceList)

    

if __name__=='__main__':
    main()